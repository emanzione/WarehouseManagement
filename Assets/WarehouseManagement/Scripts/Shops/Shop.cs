using System.Collections.Generic;
using UnityEngine;
using WarehouseManagement.Shops.Customers;
using WarehouseManagement.Warehouses;

namespace WarehouseManagement.Shops
{
    /// <summary>
    /// The shop entity. It manages customers and items exchanging
    /// with Warehouse.
    /// </summary>
    public class Shop : MonoBehaviour
    {
        /// <summary>
        /// The Warehouse this Shop can exchange items with.
        /// </summary>
        public Warehouse Warehouse;

        /// <summary>
        /// The customer prefab.
        /// </summary>
        public GameObject CustomerPrefab;

        /// <summary>
        /// Position where items will be stored.
        /// </summary>
        public Transform StoragingPosition;

        /// <summary>
        /// How many customers this shop served?
        /// </summary>
        public int SatisfiedCustomers = 0;

        /// <summary>
        /// How many items this shop successfully delivered?
        /// </summary>
        public int DeliveredItems = 0;

        private float m_x;
        private float m_y;
        private float m_z;

        private readonly Queue<Customer> m_customers = new Queue<Customer>();

        private Customer m_currentCustomer;
        private int m_storagingIndex = 0;

        protected void Start()
        {
            var bounds = GetComponent<MeshFilter>().mesh.bounds;
            m_x = bounds.size.x * transform.localScale.x / 2;
            m_y = bounds.extents.y + CustomerPrefab.transform.localScale.y;
            m_z = bounds.size.z * transform.localScale.z / 2;
        }

        protected void OnGUI()
        {
            if (GUI.Button(new Rect(10, 10, 160, 20), "Spawn customer"))
            {
                SpawnCustomer();
            }

            GUI.Label(new Rect(180, 10, 70, 20), "Capacity:");
            Warehouse.MaxCapacity = int.Parse(GUI.TextField(new Rect(250, 10, 50, 20), Warehouse.MaxCapacity.ToString()));

            GUI.Label(new Rect(10, 40, 220, 20), "Satisfied customers: " + SatisfiedCustomers);
            GUI.Label(new Rect(10, 70, 220, 20), "Delivered items: " + DeliveredItems);
            GUI.Label(new Rect(10, 100, 220, 20), "Warehouse content: " + Warehouse.StoredAmount);
            GUI.Label(new Rect(10, 130, 220, 20), "Current customers in shop: " + (m_customers.Count + ((m_currentCustomer != null) ? 1 : 0)));
            GUI.Label(new Rect(10, 160, 220, 20), "Active forklifts: " + Warehouse.ActiveForklifts);
            GUI.Label(new Rect(10, 190, 220, 20), "Manufacturers amount: " + Warehouse.Manager.ManufacturersAmount);
        }

        private void SpawnCustomer()
        {
            var position = this.transform.position + new Vector3(Random.Range(-m_x, m_x), m_y, Random.Range(-m_z, m_z));
            var customer = ((GameObject)GameObject.Instantiate(CustomerPrefab, position, Quaternion.identity)).GetComponent<Customer>();
            customer.transform.parent = this.transform;

            m_customers.Enqueue(customer);

            Warehouse.RequestItems(customer.RequestedItems);
        }
        
        protected void Update()
        {
            if (m_currentCustomer == null)
            {
                if (m_customers.Count == 0)
                {
                    if(Warehouse.StoredAmount < Warehouse.MaxCapacity)
                        if(!Warehouse.HasRequestedItems && !Warehouse.HasActiveForklifts)
                            Warehouse.RequestItems(1);

                    Warehouse.Manager.CleanManufacturers((Warehouse.StoredAmount < Warehouse.MaxCapacity) ? 1 : 0);

                    return;
                }

                m_currentCustomer = m_customers.Dequeue();
            }

            if (m_currentCustomer.IsSatisfied)
            {
                SatisfiedCustomers++;
                DeliveredItems += m_currentCustomer.RequestedItems;
                m_currentCustomer.CleanItems();
                GameObject.Destroy(m_currentCustomer.gameObject);
                m_currentCustomer = null;
                m_storagingIndex = 0;
                return;
            }

            if (Warehouse.StoredAmount > 0)
            {
                var item = Warehouse.Get();
                item.transform.parent = this.transform;
                item.transform.position = StoragingPosition.position + new Vector3(0, 1f * m_storagingIndex, 0);
                m_currentCustomer.AddItem(item);

                m_storagingIndex++;
            }
        }
    }
}