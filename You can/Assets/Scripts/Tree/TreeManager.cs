using UnityEngine;
using System.Collections.Generic;

public class TreeManager : MonoBehaviour
{
    public GameObject Planche;               // Prefab de planche à lâcher
    public List<GameObject> trees;           // Tous les arbres GameObjects (proxy si tu en utilises)
    public Transform player;
    public float activationDistance = 30f;

    private GameObject Plank;
    private Rigidbody Plank_rigidBody;
    private Vector3 PlankDirection;
    private Quaternion PlankRotation;

    private void Start()
    {
        // Optionnel : récupérer tous les arbres taggés "Arbre"
        if (trees == null || trees.Count == 0)
            trees = new List<GameObject>(GameObject.FindGameObjectsWithTag("Arbre"));
    }

    // Méthode appelée quand un arbre est frappé
    public void HitTree(GameObject tree)
    {
        if (tree == null) return;

        PlankDirection = Utilities.RandomPointAround(Vector3.zero, 1f);

        // Récupérer la position de l'arbre
        Vector3 pos = tree.transform.position;

        PlankRotation = Utilities.LookRotationY(PlankDirection);
        // Instancier une planche autour
        Plank = Instantiate(Planche, pos + PlankDirection * 0.5f + Vector3.up * 1.5f, PlankRotation);
        Plank_rigidBody = Plank.GetComponent<Rigidbody>();
        Plank_rigidBody.AddForce(PlankDirection, ForceMode.Impulse);
    }

    private void Update()
    {
        if (player == null) return;

        // Activer/désactiver les colliders des arbres selon la distance
        foreach (var tree in trees)
        {
            if (tree == null) continue;
            float distance = Vector3.Distance(player.position, tree.transform.position);
            Collider col = tree.GetComponent<Collider>();
            if (col != null)
                col.enabled = distance <= activationDistance;
        }
    }
}