using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMission : MonoBehaviour {

	public Mission currentMap;

	void Start () {
		setMission(currentMap);
	}

	void setMission(Mission mission) {
		MissionObject[] objs = Resources.FindObjectsOfTypeAll<MissionObject>();
		foreach(MissionObject obj in objs) {
			bool include = false;
			for(int i = 0; i < obj.include.Length && !include; i++)
				include = obj.include[i] == mission;

			bool exclude = false;
			for(int i = 0; i < obj.exclude.Length && !exclude; i++)
				exclude = obj.exclude[i] == mission;

			if(include && exclude)
				Debug.LogError("Object " + obj.gameObject.name + " is included and excluded at the same time in this mission!", obj.gameObject);
			
			if(include)
				obj.gameObject.SetActive(true);
			if(exclude)
				obj.gameObject.SetActive(false);
		}

	}
	
}
