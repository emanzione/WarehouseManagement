using System.Collections.Generic;
using UnityEngine;
using WarehouseManagement.Items;

namespace WarehouseManagement.Shops.Customers
{
    /// <summary>
    /// A customer entity in the shop.
    /// </summary>
    public class Customer : MonoBehaviour
    {
        /// <summary>
        /// Items this customer requested.
        /// </summary>
        public int RequestedItems;

        /// <summary>
        /// Is the customer's order satisfied?
        /// </summary>
        public bool IsSatisfied
        {
            get { return m_inventory.Count >= RequestedItems; }
        }

        private List<Item> m_inventory;

        protected void Awake()
        {
            RequestedItems = Random.Range(1, 6);
            m_inventory = new List<Item>(RequestedItems);
        }

        /// <summary>
        /// Adds an item to the customer's order.
        /// </summary>
        public void AddItem(Item item)
        {
            m_inventory.Add(item);
        }

        /// <summary>
        /// Cleans all items contained in customer's order.
        /// </summary>
        public void CleanItems()
        {
            foreach (var item in m_inventory)
            {
                GameObject.Destroy(item.gameObject);
            }
        }
    }
}