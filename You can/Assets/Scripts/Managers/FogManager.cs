using UnityEngine;

public class FogManager : MonoBehaviour
{
    [Header("Paramètres")]
    [Tooltip("Distance entre le centre et les murs/plafond")]
    public float fogDistance = 30f;

    [Header("Références")]
    public GameObject murAvant;
    public GameObject murArriere;
    public GameObject murDroite;
    public GameObject murGauche;
    public GameObject plafond;

    [Header("Dimensions")]
    [Tooltip("Hauteur des murs et plafond")]
    public float height = 10f;

    void Start()
    {
        ChangeSize(fogDistance, height);
    }

    public void ChangeSize(float fogDistance, float height)
    {
        if (!murAvant || !murArriere || !murDroite || !murGauche || !plafond)
            return;

        // --- Positionnement des murs ---
        murAvant.transform.localPosition = Vector3.forward * fogDistance;
        murArriere.transform.localPosition = Vector3.back * fogDistance;
        murDroite.transform.localPosition = Vector3.right * fogDistance;
        murGauche.transform.localPosition = Vector3.left * fogDistance;

        // Plafond au-dessus du centre
        plafond.transform.localPosition = Vector3.up * height / 2;

        // --- Orientation des murs ---
        murAvant.transform.localRotation = Quaternion.identity;
        murArriere.transform.localRotation = Quaternion.Euler(0, 180, 0);
        murDroite.transform.localRotation = Quaternion.Euler(0, 90, 0);
        murGauche.transform.localRotation = Quaternion.Euler(0, -90, 0);
        plafond.transform.localRotation = Quaternion.Euler(90, 0, 0);

        // --- Mise à l’échelle ---
        Vector3 wallScale = new Vector3(2 * fogDistance, height, 0.1f);
        murAvant.transform.localScale = wallScale;
        murArriere.transform.localScale = wallScale;

        Vector3 sideScale = new Vector3(2 * fogDistance, height, 0.1f);
        murDroite.transform.localScale = sideScale;
        murGauche.transform.localScale = sideScale;

        // Plafond couvre tout le carré formé par les murs
        plafond.transform.localScale = new Vector3(2 * fogDistance, 2 * fogDistance, 0.1f);
    }
}
