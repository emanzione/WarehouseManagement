using UnityEngine;

namespace WarehouseManagement.Manufacturers
{
    /// <summary>
    /// The manufacturer entity. It produces items at regular intervals.
    /// </summary>
    public class ItemManufacturer : MonoBehaviour
    {
        public Transform Pivot;
        public GameObject Item;
        public ManufacturerManager Manager;

        /// <summary>
        /// How many items this manufacturer produces every minute?
        /// </summary>
        public int ProductionPerMinute = 10;

        private float m_productionTickTime;
        private float m_timer;

        private bool m_isActive = true;

        private float m_itemHeight = 1f;
        
        /// <summary>
        /// Activates this manufacturer.
        /// </summary>
        public void TurnOn()
        {
            m_isActive = true;
        }

        /// <summary>
        /// Deactivates this manufacturer.
        /// </summary>
        public void TurnOff()
        {
            m_isActive = false;
        }

        protected void Awake()
        {
            m_productionTickTime = 60.0f / ProductionPerMinute;
            m_itemHeight = Item.GetComponent<MeshFilter>().sharedMesh.bounds.size.y;
            Manager = ManufacturerManager.Instance;
        }

        protected void Update()
        {
            if (m_isActive)
            {
                m_timer += Time.deltaTime;

                if (m_timer >= m_productionTickTime)
                {
                    Produce();

                    m_timer = 0f;
                }
            }
        }

        private void Produce()
        {
            Manager.Store(Item, m_itemHeight);
        }
    }
}