using UnityEngine;
using UnityEditor;

namespace prometheus
{
    [CustomEditor(typeof(MeshPreviewPRM))]
    public class MeshPreviewPRMEditor : Editor
    {
        int selectFrame = 0;

        public override void OnInspectorGUI()
        {
            MeshPreviewPRM mTarget = (MeshPreviewPRM)target;
            BuildFilesInspector(mTarget);
        }

        private void BuildFilesInspector(MeshPreviewPRM mTarget)
        {
            GUILayout.Label("SourceFrameCount:" + mTarget.sourceFrameCount);

            selectFrame = EditorGUILayout.IntSlider(selectFrame, 0, mTarget.sourceFrameCount - 1);
            if (!EditorApplication.isPlaying && mTarget.previewFrame != selectFrame)
            {
                mTarget.PreviewFrame(selectFrame);
            }

            GUILayout.Space(10);
        }
    }
}

