using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OXRTK.ARHandTracking
{
    /// <summary>
    /// The class for slate scrolling control. <br>
    /// 控制面板滚动交互的类。
    /// </summary>
    public class SlateController : MonoBehaviour
    {
        /// <summary>
        /// When the scrolling area is on the mesh other than the one on this object, use this to set the scrolling mesh. <br>
        /// 当拖拽区域为其他mesh时, 使用此处设置。<br>
        /// If the scrolling area is on the current object, leave it blank. <br>
        /// 当拖拽区域为当前物体时, 此处留空。<br>
        /// The scrolling target need to contain both meshFilter and MeshRenderer object. <br>
        /// 当拖拽区域物体必须同时包含MeshFilter和MeshRenderer来保证顺利交互。
        /// </summary>
        public GameObject scrollTargetOverride;

        /// <summary>
        /// Whether limit the scrolling area based on the texture size or not. If not, the display area depends on the wrapmode of the texture. <br>
        /// 是否限制显示区域为单张贴图范围，若为false，则显示区域取决于贴图循环模式是否为重复。<br>
        /// The start display area should be the left upper corner of texture. <br>
        /// 拖拽范围初始显示位置需要为贴图左上角。<br>
        /// If the texture tiling > 1, the limitScrollArea parameter is false. <br>
        /// 若贴图uv坐标缩放次数 > 1，则默认limitScrollArea = false。
        /// </summary>
        public bool limitScrollingArea;

        /// <summary>
        /// Whether lock the horizontal movement or not. <br>
        /// 是否锁定水平方向滚动。
        /// </summary>
        public bool lockHorizontal;

        /// <summary>
        /// Whether lock the vertical movement or not. <br>
        /// 是否锁定竖直方向滚动。
        /// </summary>
        public bool lockVertical;

        //拖拽区域的mesh材质
        private Material m_TargetMaterial;
        //拖拽区域的mesh
        private Mesh m_TargetMesh;
        //当前拖拽区域的uv
        private List<Vector2> m_CurrentUVOffsets = new List<Vector2>();
        //初始状态时拖拽区域的uv
        private List<Vector2> m_OriginalUVOffsets = new List<Vector2>();
        //当前交互位置在面板上对应的local二维点
        private Vector2 m_CurrentQuadCoord = Vector2.zero;
        //当前帧较前一帧面板上移动的对应local二维距离
        private Vector2 m_DeltaUVOffset = Vector2.zero;
        //若获取拖拽区域mesh无效，则该参数为true，不做交互计算
        private bool m_IsActive = true;
        //拖拽的Quad左下位置初始uv值
        private Vector2 m_LowerLeftCornerMinOffset = new Vector2(0, 0);
        //拖拽的Quad右上位置初始uv值
        private Vector2 m_UpperRightCornerMaxOffset = new Vector2(1, 1);

        bool m_Lock = true;

        IEnumerator moveCoroutine;

        void Start()
        {
            if (scrollTargetOverride == null && gameObject.GetComponent<MeshFilter>() != null && gameObject.GetComponent<MeshFilter>().mesh != null)
                scrollTargetOverride = gameObject;
            if (scrollTargetOverride != null)
            {
                InitMesh();
            }
            else
            {
                m_IsActive = false;
                Debug.LogError("Scrolling object do not have an scrolling content");
                return;
            }

            InitConstraint();
        }

        // Update is called once per frame
        void Update()
        {
            if (!m_IsActive)
                return;
        }

        //重置拖拽区域显示范围
        void Reset()
        {
            m_TargetMesh.SetUVs(0, m_OriginalUVOffsets);
        }

        //初始化拖拽区域mesh相关参数
        void InitMesh()
        {
            m_TargetMesh = scrollTargetOverride.GetComponent<MeshFilter>().mesh;
            m_TargetMaterial = scrollTargetOverride.GetComponent<MeshRenderer>().material;
            scrollTargetOverride.GetComponent<MeshFilter>().mesh.GetUVs(0, m_CurrentUVOffsets);
            scrollTargetOverride.GetComponent<MeshFilter>().mesh.GetUVs(0, m_OriginalUVOffsets);
        }

        //初始化显示范围限制参数
        void InitConstraint()
        {
            if (limitScrollingArea)
            {
                if (m_TargetMaterial != null)
                {
                    if (m_TargetMaterial.mainTextureScale.x > 1 || m_TargetMaterial.mainTextureScale.y > 1)
                        limitScrollingArea = false;
                    else
                    {
                        m_LowerLeftCornerMinOffset = (-1) * new Vector2(0, 1 / m_TargetMaterial.mainTextureScale.y - 1f);
                        m_UpperRightCornerMaxOffset = new Vector2(1 / m_TargetMaterial.mainTextureScale.x, 1);
                    }
                }
                else
                {
                    m_LowerLeftCornerMinOffset = new Vector2(0, 0);
                    m_UpperRightCornerMaxOffset = new Vector2(1, 1);
                }
            }
        }

        IEnumerator MoveSlate(Vector2 endCoord)
        {
            while (Vector2.Distance(m_CurrentQuadCoord, endCoord) > 0.01f)
            {
                Vector2 temp = Vector2.Lerp(m_CurrentQuadCoord, endCoord, 0.1f);
                m_DeltaUVOffset = temp - m_CurrentQuadCoord;
                m_CurrentQuadCoord = temp;

                UpdateUVOffset();
                yield return new WaitForEndOfFrame();
            }
            m_DeltaUVOffset = endCoord - m_CurrentQuadCoord;
            m_CurrentQuadCoord = endCoord;

            UpdateUVOffset();
        }

        //获取交互位置在面板上的二维点信息
        Vector2 GetQuadFromPoint(Vector3 point)
        {
            Vector2 quadCoord = Vector2.zero;
            Vector3[] vertices = m_TargetMesh.vertices;
            Vector3 upperLeft = transform.TransformPoint(vertices[2]); // slate upperLeft Point
            Vector3 upperRight = transform.TransformPoint(vertices[3]); // slate upperRight Point
            Vector3 lowerLeft = transform.TransformPoint(vertices[0]); // slate lowerLeft Point

            float magVertical = (lowerLeft - upperLeft).magnitude;
            float magHorizontal = (upperRight - upperLeft).magnitude;
            if (!Mathf.Approximately(0, magVertical) && !Mathf.Approximately(0, magHorizontal))
            {
                //获取指尖映射到图片上的位置
                quadCoord.x = Vector3.Dot(point - upperLeft, upperRight - upperLeft) / (magHorizontal * magHorizontal);
                quadCoord.y = Vector3.Dot(point - upperLeft, lowerLeft - upperLeft) / (magVertical * magVertical);
            }

            return new Vector2(lockHorizontal ? 0f : quadCoord.x, lockVertical ? 0f : quadCoord.y);
        }

        //交互过程中更新显示区域
        void UpdateUVOffset()
        {
            if (!m_IsActive)
                return;
            Vector2 textureTiling = m_TargetMaterial != null ? m_TargetMaterial.mainTextureScale : new Vector2(1f, 1f);
            Vector2 uvDelta = new Vector2(m_DeltaUVOffset.x, -m_DeltaUVOffset.y);
            if (limitScrollingArea)
            {
                bool limitX = false;
                bool limitY = false;

                for (int i = 0; i < m_CurrentUVOffsets.Count; i++)
                {
                    Vector2 tempUV = m_CurrentUVOffsets[i] - uvDelta;
                    if (tempUV.x < m_LowerLeftCornerMinOffset.x || tempUV.x > m_UpperRightCornerMaxOffset.x)
                        limitX = true;
                    if (tempUV.y < m_LowerLeftCornerMinOffset.y || tempUV.y > m_UpperRightCornerMaxOffset.y)
                        limitY = true;
                }

                for (int i = 0; i < m_CurrentUVOffsets.Count; i++)
                {
                    m_CurrentUVOffsets[i] = new Vector2(limitX ? m_CurrentUVOffsets[i].x : m_CurrentUVOffsets[i].x - uvDelta.x,
                        limitY ? m_CurrentUVOffsets[i].y : m_CurrentUVOffsets[i].y - uvDelta.y);
                }
            }
            else
            {
                for (int i = 0; i < m_CurrentUVOffsets.Count; i++)
                {
                    m_CurrentUVOffsets[i] = new Vector2(m_CurrentUVOffsets[i].x - uvDelta.x, m_CurrentUVOffsets[i].y - uvDelta.y);
                }
            }
            m_TargetMesh.SetUVs(0, m_CurrentUVOffsets);
        }

        /// <summary>
        /// Updates the interaction position when start interaction. <br>
        /// 开始交互时更新当前交互位置。
        /// </summary>
        /// <param name="pointOnSlate">The position of 3d interaction point on slate. <br>面板上的三维交互位置.</param>
        public void UpdatePointerUVStartCood(Vector3 pointOnSlate)
        {
            if (!m_IsActive)
                return;
            m_CurrentQuadCoord = GetQuadFromPoint(pointOnSlate);
            m_Lock = true;
        }

        /// <summary>
        /// Updates the interaction position during interaction. <br>
        /// 交互过程中更新当前交互位置。
        /// </summary>
        /// <param name="pointOnSlate">The position of 3d interaction point on slate. <br>面板上的三维交互位置.</param>
        /// <param name="useInertia">Uses inertia or not. <br>滚动时是否加入惯性.</param>
        public void UpdatePointerUVCoord(Vector3 pointOnSlate, bool useInertia)
        {
            if (!m_IsActive)
                return;
            Vector2 newQuadCoord = GetQuadFromPoint(pointOnSlate);
            if (m_Lock && Vector2.Distance(newQuadCoord, m_CurrentQuadCoord) > 0.05f)
                m_Lock = false;

            if (!useInertia)
            {
                m_DeltaUVOffset = newQuadCoord - m_CurrentQuadCoord;
                m_CurrentQuadCoord = newQuadCoord;

                UpdateUVOffset();
            }
            else
            {
                if (!m_Lock)
                {
                    if (moveCoroutine != null)
                        StopCoroutine(moveCoroutine);
                    moveCoroutine = MoveSlate(newQuadCoord);
                    StartCoroutine(moveCoroutine);
                }
            }
        }

    }
}