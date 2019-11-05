﻿using EPiServer.Commerce.Order;
using EPiServer.Logging;
using RestSharp;

namespace Dc.EpiServerOrderPlugin.Infrastructure
{
    public class OrderEventListener
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderEvents _orderEvents;
        private readonly ILogger _logger = LogManager.GetLogger(typeof(OrderEventListener));

        public OrderEventListener(IOrderRepository orderRepository, IOrderEvents orderEvents)
        {
            _orderRepository = orderRepository;
            _orderEvents = orderEvents;
        }

        public void AddEvents()
        {
            _orderEvents.SavedOrder += OrderEventsOnSavedOrder;
            _orderEvents.DeletingOrder += OrderEventsOnDeletingOrder;
        }

        private void OrderEventsOnSavedOrder(object sender, OrderEventArgs orderEventArgs)
        {
            var po = orderEventArgs.OrderGroup as IPurchaseOrder;
            if (po != null)
            {
                _logger.Information($"Order {po.OrderNumber} was saved");
            }
        }


        private void PopulateInfo(IPurchaseOrder order)
        {
            //basic info
            var orderNumber = order.OrderNumber;
            var orderDate = order.Created;
            var customerName = order.Name;
            var currencyCode = order.Currency.CurrencyCode;
            var currency = new Currency(currencyCode);

            var form = order.Forms.FirstOrDefault();

            var formName = form.Name;
            var subTotal = form.GetSubTotal(currency);
            var handlingTotal = form.GetHandlingTotal(currency);
            var couponCodes = form.CouponCodes;

            var payment = form.Payments.FirstOrDefault();

            //shipment info
            var shipment = form.Shipments.FirstOrDefault();
            var warehouseCode = shipment.WarehouseCode;
            var shipmentTrackingNumber = shipment.ShipmentTrackingNumber;
            var shippingMethodName = shipment.ShippingMethodName;

            var shipAdress = shipment.ShippingAddress;
            var city = shipAdress.City;
            var countryCode = shipAdress.CountryCode;
            var dayPhoneName = shipAdress.DaytimePhoneNumber;
            var eveningPhoneName = shipAdress.EveningPhoneNumber;
            var email = shipAdress.Email;
            var line1 = shipAdress.Line1;
            var line2 = shipAdress.Line2;
            var organization = shipAdress.Organization;
            var regionCode = shipAdress.RegionCode;
            var regionName = shipAdress.RegionName;


            //lineItems
            var lineItems = order.GetAllLineItems();
            var lineItem = lineItems.FirstOrDefault();

            var sku = lineItem.Code;
            var productName = lineItem.DisplayName;
            var quantity = lineItem.Quantity;
            var placedPrice = lineItem.PlacedPrice;

            var thumbnailUrl = lineItem.GetThumbnailUrl();
            var discountedPrice = lineItem.GetDiscountedPrice(currency);
            var discountedTotal = lineItem.GetDiscountTotal(currency);
            var discountedValue = lineItem.GetOrderDiscountValue();
            var fullUrl = lineItem.GetFullUrl();
            var extendedPrice = lineItem.GetExtendedPrice(currency);
        }

        private void OrderEventsOnDeletingOrder(object sender, OrderEventArgs orderEventArgs)
        {
            var cart = _orderRepository.Load<ICart>(orderEventArgs.OrderLink.OrderGroupId);
            if (cart != null)
            {
                _logger.Information($"Cart '{cart.Name}' for user '{cart.CustomerId}' is being deleted.");
            }
        }

        public void RemoveEvents()
        {
            _orderEvents.DeletingOrder -= OrderEventsOnDeletingOrder;
            _orderEvents.SavedOrder -= OrderEventsOnSavedOrder;
        }
    }
}