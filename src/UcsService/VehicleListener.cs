using System;
using System.Collections.Generic;
using UGCS.Sdk.Protocol;
using UGCS.Sdk.Protocol.Encoding;
using UGCS.UcsServices.DTO;
using UGCS.UcsServices.Enums;

namespace UGCS.UcsServices
{
    public sealed class VehicleListener
    {
        public delegate void ObjectChangeSubscriptionCallback<in T>(Sdk.Protocol.Encoding.ModificationType modification, int id, T obj) where T : IIdentifiable;

        private readonly EventSubscriptionWrapper _eventSubscriptionWrapper;
        private readonly ConnectionService _connectionService;
        private readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(VehicleListener));

        private readonly List<SubscriptionToken> tokens = new List<SubscriptionToken>();

        private NotificationHandler getObjectNotificationHandler<T>(ObjectChangeSubscriptionCallback<T> callback) where T : class, IIdentifiable
        {
            string invariantName = InvariantNames.GetInvariantName<T>();
            return notification =>
            {
                ObjectModificationEvent @event = notification.Event.ObjectModificationEvent;

                callback(@event.ModificationType, @event.ObjectId,
                    @event.ModificationType == Sdk.Protocol.Encoding.ModificationType.MT_DELETE ?
                        null : (T)@event.Object.Get(invariantName));
            };
        }

        public VehicleListener(ConnectionService cs)
        {
            _connectionService = cs;
            _eventSubscriptionWrapper = new EventSubscriptionWrapper();
        }

        public void SubscribeVehicle(System.Action<ClientVehicleDto, Enums.ModificationType> callBack)
        {
            var subscription = new ObjectModificationSubscription
            {
                ObjectType = "Vehicle"
            };

            _eventSubscriptionWrapper.ObjectModificationSubscription = subscription;

            SubscribeEventRequest requestEvent = new SubscribeEventRequest
            {
                ClientId = _connectionService.GetClientId(),

                Subscription = _eventSubscriptionWrapper
            };

            var responce = _connectionService.Submit<SubscribeEventResponse>(requestEvent);
            if (responce.Exception != null)
            {
                logger.Error(responce.Exception);
                throw new InvalidOperationException("Failed to subscribe on vehicle modifications. Try again or see log for more details.");
            }
            var subscribeEventResponse = responce.Value;

            SubscriptionToken st = new SubscriptionToken(subscribeEventResponse.SubscriptionId, getObjectNotificationHandler<Vehicle>(
                (token, vehicleId, vehicle) =>
                {
                    if (token == Sdk.Protocol.Encoding.ModificationType.MT_UPDATE || token == Sdk.Protocol.Encoding.ModificationType.MT_CREATE)
                    {
                        var newCvd = new ClientVehicleDto()
                        {
                            VehicleId = vehicle.Id,
                            Name = vehicle.Name
                        };
                        messageReceived(callBack, newCvd, Enums.ModificationType.UPDATED);
                    }
                    else
                    {
                        var newCvd = new ClientVehicleDto()
                        {
                            VehicleId = vehicleId,
                            Name = string.Empty
                        };
                        messageReceived(callBack, newCvd, Enums.ModificationType.DELETED);
                    }
                }), _eventSubscriptionWrapper);
            _connectionService.NotificationListener.AddSubscription(st);
            tokens.Add(st);
        }

        public void UnsubscribeAll()
        {
            tokens.ForEach(x => _connectionService.NotificationListener.RemoveSubscription(x, out bool removedLastForId));
        }

        private void messageReceived(System.Action<ClientVehicleDto, Enums.ModificationType> callback, ClientVehicleDto vehicle, Enums.ModificationType mtd)
        {
            callback(vehicle, mtd);
        }
    }
}
