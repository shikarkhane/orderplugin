﻿using System;
using System.Linq;
using Dc.EpiServerOrderPlugin.Extensions;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Internal;
using EPiServer.Logging;
using RestSharp;

namespace Dc.EpiServerOrderPlugin.Handlers
{
    public class OrderEventHandler : IOrderEventHandler
    {

        private static readonly ILogger Logger = LogManager.GetLogger();
        /// <summary>
        /// Call OrderRest API
        /// </summary>
        /// <param name="order"></param>
        public void PostEvent(IPurchaseOrder order)
        {
            Logger.Information($"Order has been placed: {order.OrderNumber}");

            //basic info
            var orderNumber = order.OrderNumber;
            var orderDate = order.Created;
            var customerName = order.Name;
            var currencyCode = order.Currency.CurrencyCode;
            var currency = new Mediachase.Commerce.Currency(currencyCode);
            var form = order.Forms.FirstOrDefault();
            var subTotal = form.GetSubTotal(currency);
            var handlingTotal = form.GetHandlingTotal(currency);

            //var couponCodes = form.CouponCodes;
            //var formName = form.Name;
            //var payment = form.Payments.FirstOrDefault();

            //shipment info
            var shipment = form.Shipments.FirstOrDefault();
            var warehouseCode = shipment.WarehouseCode;
            var shipmentTrackingNumber = shipment.ShipmentTrackingNumber;
            var shippingMethodName = shipment.ShippingMethodName;

            var shipAdress = shipment.ShippingAddress;
            var city = shipAdress.City;
            var countryCode = shipAdress.CountryCode;
            var daytimePhoneNumber = shipAdress.DaytimePhoneNumber;
            var eveningPhoneName = shipAdress.EveningPhoneNumber;
            var email = shipAdress.Email;
            var line1 = shipAdress.Line1;
            var line2 = shipAdress.Line2;
            var organization = shipAdress.Organization;
            var regionCode = shipAdress.RegionCode;
            var regionName = shipAdress.RegionName;
            
            //lineItems
            var lineItems = order.GetAllLineItems().Select(lineItem => new
            {
                Code = lineItem.Code,
                DisplayName = lineItem.DisplayName,
                Quantity = lineItem.Quantity,
                PlacedPrice = lineItem.PlacedPrice,
                ThumbnailUrl = lineItem.GetThumbnailUrl(),
                DiscountedPrice = lineItem.GetDiscountedPrice(currency).ToString(),
                DiscountTotal = lineItem.GetDiscountTotal(currency).ToString(),
                OrderDiscountValue = lineItem.GetOrderDiscountValue(),
                FullUrl = lineItem.GetFullUrl(),
                ExtendedPrice = lineItem.GetExtendedPrice(currency).ToString()
            }).ToList();
            

            string url = System.Web.Configuration.WebConfigurationManager.AppSettings.Get("EPi.OrderIntegration.Url");
            string resource = System.Web.Configuration.WebConfigurationManager.AppSettings.Get("EPi.OrderIntegration.Resource");
            string apiKey = System.Web.Configuration.WebConfigurationManager.AppSettings.Get("EPi.OrderIntegration.ApiKey");

            
            if (string.IsNullOrEmpty(url)) return;

            RestClient restClient = new RestClient(url);
            RestRequest restRequest = new RestRequest(resource, Method.POST);

            //Specifies request content type as Json
            restRequest.RequestFormat = DataFormat.Json;

            //Create a body with specifies parameters as json
            restRequest.AddJsonBody(new
            {
                OrderInfo = new
                {
                    OrderNumber = orderNumber,
                    CurrencyCode = currencyCode,
                    CustomerName = customerName,
                    WarehouseCode = warehouseCode,
                    HandlingTotal = handlingTotal.ToString(),
                    OrderDate = orderDate,
                    SubTotal = subTotal.ToString()
                },
                Shipment = new
                {
                    City = city,
                    CountryCode = currencyCode,
                    EveningPhoneNumber = eveningPhoneName,
                    DaytimePhoneNumber = daytimePhoneNumber,
                    RegionCode = regionCode,
                    RegionName = regionName,
                    Email = email,
                    Line1 = line1,
                    Line2 = line2,
                    ShipmentTrackingNumber = shipmentTrackingNumber,
                    ShippingMethodName = shippingMethodName
                },
                LineItems = lineItems
            });

            IRestResponse restResponse = null;

            try
            {
                restResponse = restClient.Execute(restRequest);
            }
            catch (Exception ex)
            {
                Logger.Debug(" REST API Error Message : " + ex.Message);
                if (ex.InnerException != null) Logger.Debug(" REST API Inner Exception : " + ex.InnerException.Message);
            }
            finally
            {
                if (restResponse != null) Logger.Debug(" REST API response content : " + restResponse.Content);
            }
        }

    }
}

/*
 *
 * {
    "ext_order_id": "OGrandpa_2100002",
    "currency_code": "SEK",
    "order_date": "2019-11-03 23:00:00",
    "ship_date": "2019-11-03 23:00:00",
    "billing_customer_obj": {
      "ext_customer_id": "137435",
      "name": "John Palm Wennerberg",
      "address": "F\u00e5gelstav\u00e4gen 23 ",
      "city": "Stockholm",
      "country_code": "SE",
      "email": "john@wilhlm.com",
      "zipcode": "12433"
    },
    "shipping_customer_obj": {
      "ext_customer_id": "137435",
      "name": "John Palm Wennerberg",
      "address": "F\u00e5gelstav\u00e4gen 23 ",
      "city": "Stockholm",
      "country_code": "SE",
      "email": "john@wilhlm.com",
      "zipcode": "12433"
    },
    "details_obj_list": [
      {
        "sku_number": "100382",
        "original_price": 200.0,
        "selling_price": 200.0,
        "product_id": "109335",
        "product_name": "Cotton Rib Socks 2-pack",
        "model": "Grandpa Soft Goods",
        "color": "Black",
        "size": "40-45",
        "quantity": 1,
        "category_1": "Accessoarer",
        "category_2": "Strumpor",
        "category_3": "Grandpa Soft Goods",
        "content_url": "http://www.grandpastore.se/bilder/artiklar/109335_DarkNavy.jpg",
        "description": "H\u00f6gkvalikativa strumpor fr\u00e5n Grandpa Soft Goods. 2-packEkologisk ribbstickad bomullTillverkade i Europa",
        "extra": "Cotton Rib Socks 2-pack Dark Navy, 40-45 Strumpor/Strumpbyxor Accessoarer &gt; Strumpor/Strumpbyxor",
        "barcode_number": "7340191300286",
        "product_page_url": "http://www.grandpastore.se/sv/cotton-rib-socks-2-pack-dark-navy"
      }
    ],
    "email": "john@wilhlm.com"
  },

 */
