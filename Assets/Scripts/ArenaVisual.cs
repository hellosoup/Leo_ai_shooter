using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ArenaVisual : MonoBehaviour
{
    [Serializable]
    public struct Obstacle
    {
        public Vector2 Origin;
        public Vector2[] Points;
    }

    public Transform CameraSpawnPoint;
    public Transform PlayerSpawnPoint;
    public Transform[] EnemySpawnPoints;
    public Obstacle[] Obstacles;

#if UNITY_EDITOR
    [CustomEditor(typeof(ArenaVisual)), CanEditMultipleObjects]
    public class ObstacleVisualEditor : Editor
    {
        protected virtual void OnSceneGUI()
        {
            ArenaVisual m_arenaVisual = (ArenaVisual)target;

            foreach (ref Obstacle obstacle in m_arenaVisual.Obstacles.AsSpan())
            {
                EditorGUI.BeginChangeCheck();
                Vector3 newOrigin = Handles.PositionHandle(obstacle.Origin.GetV3(), Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_arenaVisual, "Obstacle Visual Origin Set");
                    obstacle.Origin = newOrigin.GetV2();
                }

                for (int i = 0; i < obstacle.Points.Length; ++i)
                {
                    EditorGUI.BeginChangeCheck();

                    Vector3 newPoint = Handles.PositionHandle((obstacle.Origin + obstacle.Points[i]).GetV3(), Quaternion.identity);

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(m_arenaVisual, "Obstacle Visual Point Set");
                        obstacle.Points[i] = (newPoint.GetV2() - obstacle.Origin);
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
            return;

        foreach (ref Obstacle obstacle in Obstacles.AsSpan())
        {
            for (int i = 0; i < obstacle.Points.Length; ++i)
            {
                Vector3 a = (obstacle.Origin + obstacle.Points[i]).GetV3();
                Vector3 b = (obstacle.Origin + obstacle.Points[(i + 1) % obstacle.Points.Length]).GetV3();
                Gizmos.DrawLine(a, b);
            }
        }
    }
#endif
}
