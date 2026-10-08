using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotador : MonoBehaviour {

	//Velocidad de giro en cada eje (se puede cambiar desde el Inspector)
	public Vector3 velocidadGiro = new Vector3(15, 30, 45);

	// Update is called once per frame
	void Update () {

		//Rota el elemento una cantidad diferente en cada dirección y en cada intervalo de tiempo
		transform.Rotate(velocidadGiro * Time.deltaTime);
	}
}
