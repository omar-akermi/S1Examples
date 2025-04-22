using System;
using System.Collections.Generic;
using MelonLoader;
using S1API.GameTime;
using S1API.Leveling;
using S1API.NPCs;
using S1API.Products;
using S1API.Quests;
using S1API.Saveables;
using S1APIExamples;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ExampleMod.QuestTest
{
    public class CustomNPC : NPC
    {
        private const int ProductPerLevel = 5;
        private const int ProductVariation = 8;
        private const float ProductPriceMultiplier = 1.1f;

        [SaveableField("Order")]
        private OrderData? _orderData;

        public CustomNPC() : base("custom_npc", "Test", "NPC") { }

        protected override void OnCreated()
        {
            TimeManager.OnDayPass += OnDayPass;
        }

        protected override void OnResponseLoaded(Response response)
        {
            switch (response.Label)
            {
                case "ACCEPT":
                    response.OnTriggered = AcceptOrder;
                    break;
                case "DENY":
                    response.OnTriggered = DenyOrder;
                    break;
            }
        }

        private void OnDayPass()
        {
            // Only send request once per week
            if (_orderData == null && TimeManager.CurrentDay == Day.Monday)
                SendRequest();
        }

        private void SendRequest()
        {
            if (ProductManager.DiscoveredProducts.Length == 0)
                return;

            ProductDefinition product = ProductManager.DiscoveredProducts[Random.Range(0, ProductManager.DiscoveredProducts.Length)];
            int level = (int)LevelManager.Rank;
            int amount = level * ProductPerLevel + Random.Range(-ProductVariation, ProductVariation);
            int price = Mathf.RoundToInt(product.Price * amount * ProductPriceMultiplier);

            _orderData = new OrderData
            {
                Product = product,
                Amount = amount,
                Price = price
            };

            string message = $"Request: {amount}x {product.Name} for ${price}. Accept?";
            SendTextMessage(message, new[]
            {
                new Response { Label = "ACCEPT", Text = "Yes",                    OnTriggered = AcceptOrder },
                new Response { Label = "DENY", Text = "No" ,                    OnTriggered = DenyOrder }
            });
        }

        private void AcceptOrder()
        {
            if (_orderData?.Product == null)
                return;

            var quest = (OrderQuest)QuestManager.CreateQuest<OrderQuest>();
            quest.SetupAsNew(_orderData);
            _orderData = null;

            SendTextMessage("Order accepted. Quest added.");
        }

        private void DenyOrder()
        {
            _orderData = null;
            SendTextMessage("Order denied.");
        }
    }
}
