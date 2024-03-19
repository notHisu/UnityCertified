using UnityEngine;
public class BasicEnemyRotate : MonoBehaviour
{
	[SerializeField]
	float speed = 0;
	void Update()
	{
		transform.Rotate(Vector3.left * Time.deltaTime * speed);
	}
}