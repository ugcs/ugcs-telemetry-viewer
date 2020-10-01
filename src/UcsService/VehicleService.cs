using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UGCS.Sdk.Protocol;
using UGCS.Sdk.Protocol.Encoding;
using UGCS.UcsServices.DTO;
using UGCS.UcsServices.Enums;

namespace UGCS.UcsServices
{
    public sealed class VehicleService : IDisposable
    {
        private const string PREF_NAME = "mission";
        private const String VEHICLE_PATTERN = @"<selection +selection=\""(\d*)\"" *\/>";
        private const String INVARIANT_ENTITY_NAME = "Vehicle";

        private readonly VehicleListener _vehicleListener;
        public delegate void VehicleUpdated(ClientVehicleDto vehicle);
        public event VehicleUpdated OnVehicleUpdated;
        private readonly ILog _logger = LogManager.GetLogger(typeof(VehicleService));
        private readonly ConnectionService _connectionService;
        private ConcurrentDictionary<int, ClientVehicleDto> Vehicles { get; set; } = new ConcurrentDictionary<int, ClientVehicleDto>();
        private bool _isDisposed = false;

        public bool EnableVehicleSynchronisation { get; set; }

        public VehicleService(ConnectionService cs, VehicleListener vl)
        {
            EnableVehicleSynchronisation = true; //hardcode config
            _connectionService = cs;
            cs.Connected += ucs_Connected;
            cs.Disconnected += ucs_Disconnected;

            _vehicleListener = vl;

            if (cs.IsConnected)
            {
                try
                {
                    updateAvailableVehicles();
                    _vehicleListener.SubscribeVehicle(refreshVehicle);
                }
                catch (Exception err)
                {
                    _logger.Error("Error occured.", err);
                }
            }
        }

        private void ucs_Disconnected(object sender, EventArgs e)
        {
            _vehicleListener.UnsubscribeAll();
        }

        private void ucs_Connected(object sender, EventArgs e)
        {
            try
            {
                updateAvailableVehicles();
                _vehicleListener.SubscribeVehicle(refreshVehicle);
            }
            catch (Exception err)
            {
                _logger.Error("Error occured.", err);
            }
        }
        public List<ClientVehicleDto> GetVehicles()
        {
            List<ClientVehicleDto> ret = new List<ClientVehicleDto>();
            foreach (var vehicle in Vehicles)
            {
                ret.Add(new ClientVehicleDto()
                {
                    VehicleId = vehicle.Value.VehicleId,
                    Name = vehicle.Value.Name
                });
            }
            return ret;

        }
        private void updateAvailableVehicles()
        {
            ConcurrentDictionary<int, ClientVehicleDto> res = new ConcurrentDictionary<int, ClientVehicleDto>();
            GetObjectListRequest request = new GetObjectListRequest()
            {
                ClientId = _connectionService.GetClientId(),
                ObjectType = INVARIANT_ENTITY_NAME,
                RefreshDependencies = true,
            };
            request.RefreshExcludes.Add("PayloadProfile");
            request.RefreshExcludes.Add("Route");
            request.RefreshExcludes.Add("Mission");
            request.RefreshExcludes.Add("Platform");

            var response = _connectionService.Execute<GetObjectListResponse>(request);

            foreach (var vehicles in response.Objects)
            {
                res.TryAdd(vehicles.Vehicle.Id, new ClientVehicleDto()
                {
                    VehicleId = vehicles.Vehicle.Id,
                    Name = vehicles.Vehicle.Name
                });
            }
            Vehicles = res;
        }

        private void refreshVehicle(ClientVehicleDto vehicle, Enums.ModificationType mtd)
        {
            bool update = false;
            if (!Vehicles.ContainsKey(vehicle.VehicleId))
            {
                update = true;
            }
            Vehicles.AddOrUpdate(vehicle.VehicleId, vehicle, (key, oldValue) =>
            {
                if (vehicle.Name != oldValue.Name)
                {
                    update = true;
                }
                return vehicle;
            });
            if (update && OnVehicleUpdated != null)
            {
                OnVehicleUpdated(vehicle);
            }
        }

        /// <summary>
        /// Create subscribtion to change the selected vehicle
        /// </summary>
        /// <param name="handler">Handler for new vehicle tail number</param>
        /// <returns>Returns subscription id or null if error is raised</returns>
        public int SubscribeSelectedVehicleChange(System.Action<ClientVehicleDto> handler)
        {
            //get selected vehicle id from mission preferences
            ObjectModificationSubscription missionPrefSubscription = new ObjectModificationSubscription
            {
                ObjectType = InvariantNames.GetInvariantName<MissionPreference>()
            };

            EventSubscriptionWrapper subscriptionWrapper = new EventSubscriptionWrapper
            {
                ObjectModificationSubscription = missionPrefSubscription
            };

            var response = _connectionService.Execute<SubscribeEventResponse>(
                new SubscribeEventRequest()
                {
                    ClientId = _connectionService.GetClientId(),
                    Subscription = subscriptionWrapper,
                });
            _connectionService.NotificationListener.AddSubscription(new SubscriptionToken(response.SubscriptionId,
                (notif) =>
                {
                    var prefs = notif.Event.ObjectModificationEvent.Object.MissionPreference;
                    if (prefs.Name.Equals(PREF_NAME))
                    {
                        int? id = missionPreferenceToVehicleId(prefs, _connectionService.GetUser().Id);
                        if (id != null)
                        {
                            var v = getVehicleById(id.Value);
                            if (v != null)
                            {
                                handler(new ClientVehicleDto()
                                {
                                    Name = v.Name,
                                    VehicleId = v.Id
                                });
                                return;
                            }
                        }
                        handler(null);
                    }
                },
                subscriptionWrapper));
            return response.SubscriptionId;
        }

        /// <summary>
        /// Gets selected vehicle.
        /// </summary>
        /// <returns>Return <see cref="null"/> if there are no info about selected vehicle, otherwise returns <see cref="ClientVehicleDto"/> object.</returns>
        public ClientVehicleDto GetSelectedVehicleId()
        {
            if (!_connectionService.IsConnected)
                return null;

            var getMissionReq = new GetMissionPreferencesRequest()
            {
                User = _connectionService.GetUser(),
                ClientId = _connectionService.GetClientId(),
                Mission = null,
            };
            var getMissionResp = _connectionService.Execute<GetMissionPreferencesResponse>(getMissionReq);

            MissionPreference pref = getMissionResp.Preferences.FirstOrDefault(p => p.Name == PREF_NAME);
            int? id = missionPreferenceToVehicleId(pref, getMissionReq.User.Id);
            if (id != null)
            {
                var v = getVehicleById(id.Value);
                if (v != null)
                {
                    return new ClientVehicleDto()
                    {
                        Name = v.Name,
                        VehicleId = v.Id
                    };
                }
            }
            return null;
        }

        private int? missionPreferenceToVehicleId(MissionPreference prefs, int userId)
        {
            if (prefs == null)
                return null;
            if (EnableVehicleSynchronisation &&
                prefs.User.Id == userId)
            {
                string selectionValue = prefs.Value;
                Regex r = new Regex(VEHICLE_PATTERN, RegexOptions.IgnoreCase);
                Match match = r.Match(selectionValue);
                if (match.Success)
                {
                    if (!string.IsNullOrEmpty(match.Groups[1].Captures[0].Value))
                        return int.Parse(match.Groups[1].Captures[0].Value);
                    else
                        return null;
                }
            }
            return null;
        }

        private Vehicle getVehicleById(int id)
        {
            var getVehicleResp = _connectionService.Execute<GetObjectResponse>(
                new GetObjectRequest
                {
                    ClientId = _connectionService.GetClientId(),
                    ObjectType = InvariantNames.GetInvariantName<Vehicle>(),
                    ObjectId = id
                });

            var vehicle = getVehicleResp.Object?.Vehicle;
            return vehicle;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _connectionService.Connected -= ucs_Connected;
            _vehicleListener.UnsubscribeAll();

            _isDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
