using UnityEngine.SceneManagement;
using UnityEngine;

public class TitleComponent : MonoBehaviour
{
  void Update()
  {
    if (Input.GetMouseButtonUp(0))
    {
      SceneManager.LoadScene("shop");
    }
  }
}