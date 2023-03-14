using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachToTransform : MonoBehaviour
{
    [SerializeField] private Transform m_transform;
    private void Awake()
    {
        if(m_transform!=null) transform.SetParent(m_transform,true);
    }

}
