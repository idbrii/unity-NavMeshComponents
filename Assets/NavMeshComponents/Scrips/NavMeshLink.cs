using System.Collections.Generic;

namespace UnityEngine.AI
{
    [AddComponentMenu("Navigation/NavMeshLink", 33)]
    [ExecuteInEditMode]
    [DefaultExecutionOrder(-101)]
    public class NavMeshLink : MonoBehaviour
    {
        [SerializeField]
        int m_AgentTypeID;
        public int agentTypeID { get { return m_AgentTypeID; } set { m_AgentTypeID = value; UpdateLink(); } }

        [SerializeField]
        Vector3 m_StartPoint = new Vector3(0.0f, 0.0f, -2.5f);
        public Vector3 startPoint { get { return m_StartPoint; } set { m_StartPoint = value; UpdateLink(); } }

        [SerializeField]
        Vector3 m_EndPoint = new Vector3(0.0f, 0.0f, 2.5f);
        public Vector3 endPoint { get { return m_EndPoint; } set { m_EndPoint = value; UpdateLink(); } }

        [SerializeField]
        float m_Width;
        public float width { get { return m_Width; } set { m_Width = value; UpdateLink(); } }

        [SerializeField]
        bool m_Bidirectional = true;
        public bool bidirectional { get { return m_Bidirectional; } set { m_Bidirectional = value; UpdateLink(); } }

        [SerializeField]
        bool m_AutoUpdatePosition;
        public bool autoUpdate { get { return m_AutoUpdatePosition; } set { SetAutoUpdate(value); } }

        [SerializeField]
        int m_Area;
        public int area { get { return m_Area; } set { m_Area = value; UpdateLink(); } }

        NavMeshLinkInstance m_LinkInstance = new NavMeshLinkInstance();

        Vector3 m_LastPosition = Vector3.zero;
        Quaternion m_LastRotation = Quaternion.identity;
        Vector3 m_LastScale = Vector3.one;

        static readonly List<NavMeshLink> s_Tracked = new List<NavMeshLink>();

        void OnEnable()
        {
            AddLink();
            if (m_AutoUpdatePosition && m_LinkInstance.valid)
                AddTracking(this);
        }

        void OnDisable()
        {
            RemoveTracking(this);
            m_LinkInstance.Remove();
        }

        public void UpdateLink()
        {
            m_LinkInstance.Remove();
            AddLink();
        }

        static void AddTracking(NavMeshLink link)
        {
#if UNITY_EDITOR
            if (s_Tracked.Contains(link))
            {
                Debug.LogError("Link is already tracked: " + link);
                return;
            }
#endif

            if (s_Tracked.Count == 0)
                NavMesh.onPreUpdate += UpdateTrackedInstances;

            s_Tracked.Add(link);
        }

        static void RemoveTracking(NavMeshLink link)
        {
            s_Tracked.Remove(link);

            if (s_Tracked.Count == 0)
                NavMesh.onPreUpdate -= UpdateTrackedInstances;
        }

        void SetAutoUpdate(bool value)
        {
            if (m_AutoUpdatePosition == value)
                return;
            m_AutoUpdatePosition = value;
            if (value)
                AddTracking(this);
            else
                RemoveTracking(this);
        }

        void AddLink()
        {
#if UNITY_EDITOR
            if (m_LinkInstance.valid)
            {
                Debug.LogError("Link is already added: " + this);
                return;
            }
#endif

            var link = new NavMeshLinkData();
            link.startPosition = transform.TransformPoint(m_StartPoint);
            link.endPosition = transform.TransformPoint(m_EndPoint);
            link.width = m_Width;
            link.upAxis = transform.up;
            link.costModifier = -1.0f;
            link.bidirectional = m_Bidirectional;
            link.area = m_Area;
            link.agentTypeID = m_AgentTypeID;
            m_LinkInstance = NavMesh.AddLink(link);
            if (m_LinkInstance.valid)
                m_LinkInstance.owner = this;

            m_LastPosition = transform.position;
            m_LastRotation = transform.rotation;
            m_LastScale = transform.lossyScale;
        }

        bool HasTransformChanged()
        {
            if (m_LastPosition != transform.position) return true;
            if (m_LastRotation != transform.rotation) return true;
            if (m_LastScale != transform.lossyScale) return true;
            return false;
        }

        void OnDidApplyAnimationProperties()
        {
            UpdateLink();
        }

        static void UpdateTrackedInstances()
        {
            foreach (var instance in s_Tracked)
            {
                if (instance.HasTransformChanged())
                    instance.UpdateLink();
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (!m_LinkInstance.valid)
                return;

            UpdateLink();

            if (!m_AutoUpdatePosition)
            {
                RemoveTracking(this);
            }
            else if (!s_Tracked.Contains(this))
            {
                AddTracking(this);
            }
        }
#endif
    }
}
