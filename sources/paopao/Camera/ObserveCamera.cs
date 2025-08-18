using System;
using UnityEngine;

public class ObserveCamera : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		if (Input.acceleration.x > this.deadZone)
		{
			this.x += this.spin;
		}
		else if (Input.acceleration.x < -this.deadZone)
		{
			this.x -= this.spin;
		}
		if (Input.acceleration.y > this.deadZone)
		{
			this.y -= this.spin;
		}
		else if (Input.acceleration.y < -this.deadZone)
		{
			this.y += this.spin;
		}
		base.transform.rotation = Quaternion.Euler(this.x, this.y, this.z);
		if (Input.touchCount > 0)
		{
			if (Input.touchCount == 1)
			{
				if (Input.touches[0].position.x < (float)(Screen.width / 2))
				{
					Vector3 vector = base.camera.transform.TransformDirection(Vector3.forward);
					vector.Normalize();
					base.transform.position = base.transform.position + vector * this.speed;
				}
				else if (Input.touches[0].position.x > (float)(Screen.width / 2))
				{
					Vector3 vector2 = base.camera.transform.TransformDirection(Vector3.forward);
					vector2.Normalize();
					base.transform.position = base.transform.position - vector2 * this.speed;
				}
			}
			else if (Input.touchCount == 2)
			{
				base.transform.position = new Vector3(0.03010796f, 0.819397f, 3.523256f);
				this.x = 0f;
				this.y = 180f;
				this.z = 0f;
				base.transform.rotation = Quaternion.Euler(this.x, this.y, this.z);
			}
		}
	}

	private float spin = 1.5f;

	private float speed = 0.5f;

	private float deadZone = 0.3f;

	private float x;

	private float y = 180f;

	private float z;
}
