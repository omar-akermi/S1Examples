using System;
using System.Collections.Generic;
using System.Linq;
using MelonLoader;
using S1API.DeadDrops;
using S1API.Items;
using S1API.Money;
using S1API.Products;
using S1API.Quests;
using S1API.Saveables;
using ScheduleOneEnhanced.Mods.BulkBuyer;
using UnityEngine;
using OrderData = S1APIExamples.OrderData;
using Random = UnityEngine.Random;

namespace ExampleMod.QuestTest
{
    class OrderQuest : Quest
    {
        protected override string Title => "Delivery Request";

        protected override string Description =>
            $"Deliver the requested product and collect <color=#00CC00>${_orderData.Price:N0}</color> from the drop.";

        [SaveableField("Order")]
        private OrderData _orderData = new();

        [SaveableField("DropData")]
        private DeadDropData _deadDropData = new();

        private QuestEntry? _deliveryEntry;
        private QuestEntry? _rewardEntry;

        protected override void OnCreated()
        {
            _deliveryEntry = AddEntry(
                $"Deliver {_orderData.Amount}x <color=#3399FF>{_orderData.Product?.Name}</color>.",
                _deadDropData.DeliveryDeadDrop?.Position
            );

            _rewardEntry = AddEntry(
                $"Collect <color=#00CC00>${_orderData.Price:N0}</color>.",
                _deadDropData.CollectionDeadDrop?.Position
            );

            if (_deadDropData.DeliveryDeadDrop != null)
                _deadDropData.DeliveryDeadDrop.Storage.OnClosed += TryCompleteDelivery;

            if (_deadDropData.CollectionDeadDrop != null)
                _deadDropData.CollectionDeadDrop.Storage.OnOpened += TryCollectReward;
        }

        public void SetupAsNew(OrderData orderData)
        {
            _orderData = orderData;
            _deadDropData.DeliveryDeadDrop = GetRandomDrop();
            _deadDropData.CollectionDeadDrop = GetRandomDrop();
        }

        private DeadDropInstance GetRandomDrop()
        {
            return DeadDropManager.All[1];
        }

        private void TryCompleteDelivery()
        {
            if (_orderData.Product == null || _deadDropData.DeliveryDeadDrop == null) return;

            var slots = _deadDropData.DeliveryDeadDrop.Storage.Slots
                .Where(slot => slot.ItemInstance is ProductInstance { IsPackaged: true } product &&
                               product.Definition == _orderData.Product)
                .ToList();

            int total = slots.Sum(slot =>
                slot.Quantity * ((ProductInstance)slot.ItemInstance).AppliedPackaging.Quantity);

            if (total < _orderData.Amount) return;

            int remaining = _orderData.Amount;
            foreach (var slot in slots)
            {
                var product = (ProductInstance)slot.ItemInstance;
                int packagedQty = product.AppliedPackaging.Quantity;
                int toRemove = slot.Quantity * packagedQty;
                slot.AddQuantity(-(toRemove / packagedQty));
                remaining -= toRemove;
            }

            _deliveryEntry?.Complete();
            _rewardEntry?.SetState(QuestState.Active);
        }

        private void TryCollectReward()
        {
            if (_orderData == null || _deadDropData.CollectionDeadDrop == null) return;

            var cashDef = (CashDefinition)ItemManager.GetItemDefinition("cash");
            var cash = (CashInstance)cashDef.CreateInstance();
            cash.SetQuantity(_orderData.Price);

            if (!_deadDropData.CollectionDeadDrop.Storage.CanItemFit(cash))
                return;

            _deadDropData.CollectionDeadDrop.Storage.AddItem(cash);
            _rewardEntry?.Complete();
        }
    }
}
