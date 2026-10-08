using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Música de ambiente que sigue sonando al cambiar de escena
[RequireComponent(typeof(AudioSource))]
public class MusicaAmbiente : MonoBehaviour {

	private static MusicaAmbiente instancia;

	void Awake () {

		//Si ya hay música sonando de otra escena, no creamos otra
		if (instancia != null) {
			Destroy(gameObject);
			return;
		}

		instancia = this;
		DontDestroyOnLoad(gameObject);

		AudioSource audio = GetComponent<AudioSource>();
		audio.loop = true;
		if (!audio.isPlaying) audio.Play();
	}
}
