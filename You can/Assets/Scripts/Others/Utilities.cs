using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public static class Utilities
{
    

    // --- Caméra principale ---
    public static void SetMainCamera(Camera newMainCam)
    {
        Camera[] allCameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);

        foreach (Camera cam in allCameras)
        {
            cam.gameObject.SetActive(false);

            if (cam.CompareTag("MainCamera"))
            {
                cam.tag = "Untagged";
            }
        }

        newMainCam.gameObject.SetActive(true);
        newMainCam.tag = "MainCamera";
    }

    public static void DisableAllExcept(List<GameObject> allowedObjects)
    {
        HashSet<GameObject> allowedSet = new HashSet<GameObject>(allowedObjects);

        bool IsAllowed(GameObject obj)
        {
            Transform t = obj.transform;
            while (t != null)
            {
                if (allowedSet.Contains(t.gameObject)) return true;
                t = t.parent;
            }
            return false;
        }

        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (obj == null) continue;
            if (IsAllowed(obj)) continue;

            obj.SetActive(false);
        }
    }
    public static void EnableAll()
    {
        int sceneCount = SceneManager.sceneCount;

        for (int i = 0; i < sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            if (!scene.isLoaded)
                continue;

            GameObject[] rootObjects = scene.GetRootGameObjects();

            foreach (GameObject root in rootObjects)
            {
                EnableRecursively(root);
            }
        }
    }
    private static void EnableRecursively(GameObject obj)
    {
        obj.SetActive(true);

        foreach (Transform child in obj.transform)
        {
            EnableRecursively(child.gameObject);
        }
    }
    public static void EnableAllExcept(List<GameObject> excludedObjects)
    {
        HashSet<GameObject> excludedSet = new HashSet<GameObject>(excludedObjects);

        bool IsExcluded(GameObject obj)
        {
            Transform t = obj.transform;
            while (t != null)
            {
                if (excludedSet.Contains(t.gameObject)) return true;
                t = t.parent;
            }
            return false;
        }

        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (obj == null) continue;
            if (IsExcluded(obj)) continue;

            obj.SetActive(true);
        }
    }

    // --- Coroutines ---
    public static void ScaleImageOverTime(Image targetObject, Vector3 endScale, float time)
    {
        CoroutineRunner.Run(ScaleImageRoutine(targetObject, endScale, time));
    }

    private static IEnumerator ScaleImageRoutine(Image targetObject, Vector3 endScale, float time)
    {
        Vector3 startScale = targetObject.transform.localScale;
        float elapsed = 0f;

        while (elapsed < time)
        {
            targetObject.transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / time);
            elapsed += Time.deltaTime;
            yield return null;
        }

        targetObject.transform.localScale = endScale;
    }

    public static void ScaleObjectOverTime(GameObject targetObject, Vector3 endScale, float time)
    {
        CoroutineRunner.Run(ScaleObjectRoutine(targetObject, endScale, time));
    }

    private static IEnumerator ScaleObjectRoutine(GameObject targetObject, Vector3 endScale, float time)
    {
        if (targetObject == null)
            yield return null;
        Vector3 startScale = targetObject.transform.localScale;
        float elapsed = 0f;

        while (elapsed < time)
        {
            targetObject.transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / time);
            elapsed += Time.deltaTime;
            yield return null;
        }

        targetObject.transform.localScale = endScale;
    }

    public static void RotateObject360OverTime(GameObject targetObject, float time)
    {

        CoroutineRunner.Run(RotateObjectRoutine(targetObject, time));
    }

    private static IEnumerator RotateObjectRoutine(GameObject targetObject, float time)
    {
        if (targetObject == null)
            yield break;
        Quaternion startRotation = targetObject.transform.localRotation;
        float elapsed = 0f;

        while (elapsed < time)
        {
            if (targetObject == null)
                yield break;
            targetObject.transform.localRotation = startRotation * Quaternion.Euler(0, 360 * (elapsed / time), 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        targetObject.transform.localRotation = startRotation;
    }

    // --- Activation / désactivation de scripts ---
    public static void DisableAllScriptsExcept(List<GameObject> allowedGameObjects)
    {
        MonoBehaviour[] allScripts = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        HashSet<MonoBehaviour> allowedScripts = new HashSet<MonoBehaviour>();

        foreach (GameObject go in allowedGameObjects)
        {
            MonoBehaviour[] scriptsInGO = go.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (MonoBehaviour script in scriptsInGO)
                allowedScripts.Add(script);
        }

        foreach (MonoBehaviour script in allScripts)
        {
            if (allowedScripts.Contains(script)) continue;
            script.enabled = false;
        }
    }

    public static void EnableAllScriptsExcept(List<GameObject> allowedGameObjects)
    {
        // Crée un HashSet contenant tous les scripts autorisés
        HashSet<MonoBehaviour> allowedScripts = new HashSet<MonoBehaviour>();
        foreach (GameObject go in allowedGameObjects)
        {
            if (go == null) continue;
            MonoBehaviour[] scripts = go.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (MonoBehaviour script in scripts)
            {
                if (script != null)
                    allowedScripts.Add(script);
            }
        }

        // Récupère tous les MonoBehaviour de la scène, même ceux inactifs
        MonoBehaviour[] allScripts = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        // Active tous les scripts qui ne sont pas dans la liste autorisée
        foreach (MonoBehaviour script in allScripts)
        {
            if (script == null) continue;
            if (!allowedScripts.Contains(script))
                script.enabled = true;
        }
    }

    // --- Points aléatoires ---
    public static Vector3 RandomPointAround(Vector3 center, float distance)
    {
        float angle = Random.Range(0f, 2f * Mathf.PI);
        float offsetX = Mathf.Cos(angle) * distance;
        float offsetZ = Mathf.Sin(angle) * distance;

        float clampedX = Mathf.Clamp(center.x + offsetX, -125f, 125f);
        float clampedZ = Mathf.Clamp(center.z + offsetZ, -125f, 125f);

        return new Vector3(clampedX, 1f, clampedZ);
    }

    public static void DisableScript(MonoBehaviour script)
    {
        if (script != null)
            script.enabled = false;
    }

    public static void EnableScript(MonoBehaviour script)
    {
        if (script != null)
            script.enabled = true;
    }

    public static Quaternion LookRotationY(Vector3 direction)
    {
        // On ignore la composante Y pour ne tourner que sur l’axe vertical
        direction.y = 0f;

        // Si la direction est trop faible, on retourne une rotation neutre
        if (direction.sqrMagnitude < 0.0001f)
            return Quaternion.identity;

        // Calcule la rotation qui "regarde" dans la direction donnée
        return Quaternion.LookRotation(direction.normalized, Vector3.up);
    }
}