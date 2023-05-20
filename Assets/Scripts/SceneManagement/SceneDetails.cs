using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDetails : MonoBehaviour
{
    [SerializeField] List<SceneDetails> connectedScenes;
    [SerializeField] AudioClip sceneMusic;

    public bool IsLoaded { get; private set; }

    List<SavableEntity> savableEntities;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            Debug.Log($"Entered {gameObject.name}");

            LoadScene();
            GameController.Instance.SetCurrentScene(this);

            if (sceneMusic != null )
                AudioManager.i.PlayMusic(sceneMusic, fade: true);

            //Cargar todas las escenas conectadas
            foreach (var scene in connectedScenes)
            {
                scene.LoadScene();
            }

            //Borrar escenas que no estan conectadas
            var prevScene = GameController.Instance.PrevScene;
            if(prevScene != null)
            {
                var previoslyLoadScenes = GameController.Instance.PrevScene.connectedScenes;
                foreach (var scene in previoslyLoadScenes)
                {
                    if (!connectedScenes.Contains(scene) && scene != this)
                        scene.UnloadScene();
                }

                if (!connectedScenes.Contains(prevScene))
                    prevScene.UnloadScene();
            }
        }
    }

    public void LoadScene()
    {
        if (!IsLoaded)
        {
            var operation = SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
            IsLoaded = true;

            operation.completed += (AsyncOperation op) =>
            {
                savableEntities = GetSavableEntitiesInSceneInactive();
                SavingSystem.i.RestoreEntityStates(savableEntities);
            };
        }
    }

    public void UnloadScene()
    {
        if (IsLoaded)
        {
            SavingSystem.i.CaptureEntityStates(savableEntities);
            
            SceneManager.UnloadSceneAsync(gameObject.name);
            IsLoaded = false;
        }
    }

    List<SavableEntity> GetSavableEntitiesInScene()
    {
        var currScene = SceneManager.GetSceneByName(gameObject.name);
        var savableEntities = FindObjectsOfType<SavableEntity>().Where(x => x.gameObject.scene == currScene).ToList();

        for (int i = 0; i < savableEntities.Count; i++)
        {
            Debug.Log("Entidad: " + savableEntities[i].name + " En escena: " + currScene.name);
        }

        return savableEntities;
    }

    List<SavableEntity> GetSavableEntitiesInSceneInactive()
    {
        var currScene = SceneManager.GetSceneByName(gameObject.name);
        var savableEntities = new List<SavableEntity>();
        var allObjects = Resources.FindObjectsOfTypeAll<SavableEntity>().ToList();
        foreach (var entity in allObjects)
        {
            if (entity.gameObject.scene == currScene)
            {
                savableEntities.Add(entity);
            }
        }
        return savableEntities;
    }

    public AudioClip SceneMusic => sceneMusic;
}
