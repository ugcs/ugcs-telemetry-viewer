using System;
using System.Collections.Generic;
using Com.Ugcs.Ucs.Proto;
using UGCS.UcsServices.DTO;
using UGCS.UcsServices.Enums;

namespace UGCS.UcsServices
{
    public sealed class VehicleListener
    {
        private readonly EventSubscriptionWrapper _eventSubscriptionWrapper;
        private readonly ConnectionService _connectionService;
        private readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(VehicleListener));

        private readonly List<SubscriptionToken> tokens = new List<SubscriptionToken>();

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

            var responce = _connectionService.Submit<SubscribeEventRequest, SubscribeEventResponse>(requestEvent);
            if (responce.Exception != null)
            {
                logger.Error(responce.Exception);
                throw new InvalidOperationException("Failed to subscribe on vehicle modifications. Try again or see log for more details.");
            }
            var subscribeEventResponse = responce.Value;

            NotificationHandler handler = notification =>
            {
                ObjectModificationEvent @event = notification.Event.ObjectModificationEvent;

                if (@event.ModificationType == Com.Ugcs.Ucs.Proto.ModificationType.MtUpdate || @event.ModificationType == Com.Ugcs.Ucs.Proto.ModificationType.MtCreate)
                {
                    Vehicle vehicle = @event.Object.Vehicle;
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
                        VehicleId = @event.ObjectId,
                        Name = string.Empty
                    };
                    messageReceived(callBack, newCvd, Enums.ModificationType.DELETED);
                }
            };

            SubscriptionToken st = new SubscriptionToken(subscribeEventResponse.SubscriptionId, handler, _eventSubscriptionWrapper);
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
