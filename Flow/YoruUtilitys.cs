using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

static public class YoruUtilitys
{
    // 유니티 오브젝트의 트리구조를 재귀탐색해서, 전부 리스트로 반환한다
    // IEnumerable: 포이치로 돌릴 수 있는 클래스. 재귀탐색용
    public static IEnumerable<Transform> GetAllTransforms(Transform root)
    {
        yield return root; // 자신

        foreach (Transform child in root)
        {
            foreach (var t in GetAllTransforms(child))
                yield return t;
        }
    }

    // 유니티 씬의 루트 하위를 스캔하여 노드 루트들을 트리구조로 수집해온다 
    public static List<NodeLegacy> NodeInstaller(Transform root)
    {
        List<Transform> listTransforms = new List<Transform>();
        foreach (Transform t in GetAllTransforms(root))
        {
            listTransforms.Add(t);
        }
        List<NodeLegacy> nodes = new List<NodeLegacy>();
        foreach (var obj in listTransforms)
        {
            if (obj.GetComponent<Image>() != null)
            {
                Image image = obj.GetComponent<Image>();
                NodeLegacy node = new NodeImage(1, "qwe", image, obj.gameObject);
                nodes.Add(node);
            }
            else
            {
                NodeLegacy node = new NodeEmpty(1, "qwe", obj.gameObject);
                nodes.Add(node);
            }
        }
        return nodes;
    }



}
