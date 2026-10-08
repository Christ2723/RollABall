using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Mueve un obstáculo de un lado a otro y/o lo hace girar
public class ObstaculoMovil : MonoBehaviour {

	//Distancia y dirección del recorrido desde la posición inicial
	public Vector3 recorrido = new Vector3(6, 0, 0);

	//Cuántos segundos tarda en ir y volver
	public float duracionCiclo = 4f;

	//Desfase para que no todos los obstáculos vayan sincronizados (0 a 1)
	public float desfase = 0f;

	//Grados por segundo que gira sobre el eje Y (0 = no gira)
	public float velocidadGiro = 0f;

	private Rigidbody rb;
	private Vector3 posicionInicial;

	void Start () {

		rb = GetComponent<Rigidbody>();
		posicionInicial = transform.position;
	}

	// Usamos FixedUpdate porque movemos un Rigidbody cinemático
	void FixedUpdate () {

		//Seno entre -1 y 1 para ir y volver suavemente
		float t = Mathf.Sin((Time.time / duracionCiclo + desfase) * Mathf.PI * 2f);
		rb.MovePosition(posicionInicial + recorrido * t);

		if (velocidadGiro != 0f) {
			rb.MoveRotation(rb.rotation * Quaternion.Euler(0, velocidadGiro * Time.fixedDeltaTime, 0));
		}
	}
}
