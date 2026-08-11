using System;
using SFS.Translations;
using SFS.UI;
using SFS.WorldBase;
using UnityEngine;

namespace SFS.World.Maps;

public class Landmark : MonoBehaviour
{
	public LandmarkData data;

	public Double2 position;

	public Field displayName;

	public void Initialize(LandmarkData data, Planet planet)
	{
		this.data = data;
		position = Double2.CosSin(MathF.PI / 180f * data.Center) * (planet.Radius + planet.GetTerrainHeightAtAngle(data.Center * (MathF.PI / 180f), clampToWater: true));
		Loc.OnChange += new Action(UpdateName);
	}

	private void OnDestroy()
	{
		Loc.OnChange -= new Action(UpdateName);
	}

	private void UpdateName()
	{
		displayName = GetDisplayName(data.name);
	}

	private static Field GetDisplayName(string codeName)
	{
		SFS_Translation main = Loc.main;
		return codeName switch
		{
			"Space Center" => main.Go_To_Space_Center, 
			"Sea of Tranquility" => main.Sea_of_Tranquility, 
			"Sea of Serenity" => main.Sea_of_Serenity, 
			"Ocean of Storms" => main.Ocean_of_Storms, 
			"Copernicus Crater" => main.Copernicus_Crater, 
			"Tycho Crater" => main.Tycho_Crater, 
			"Olympus Mons" => main.Olympus_Mons, 
			"Valles Marineris" => main.Valles_Marineris, 
			"Gale Crater" => main.Gale_Crater, 
			"Hellas Planitia" => main.Hellas_Planitia, 
			"Arcadia Planitia" => main.Arcadia_Planitia, 
			"Utopia Planitia" => main.Utopia_Planitia, 
			"Jezero Crater" => main.Jezero_Crater, 
			"Stickney Crater" => main.Stickney_Crater, 
			"Voltaire Crater" => main.Voltaire_Crater, 
			"Swift Crater" => main.Swift_Crater, 
			"Atalanta Planitia" => main.Atalanta_Planitia, 
			"Lavinia Planitia" => main.Lavinia_Planitia, 
			"Maxwell Montes" => main.Maxwell_Montes, 
			"Caloris Planitia" => main.Caloris_Planitia, 
			"Borealis Planitia" => main.Borealis_Planitia, 
			"Verona Rupes" => main.Verona_Rupes, 
			"Arden Corona" => main.Arden_Corona, 
			"Elsinore Corona" => main.Elsinore_Corona, 
			"Mommur Chasma" => main.Mommur_Chasma, 
			"Wunda Crater" => main.Wunda_Crater, 
			"Vuver Crater" => main.Vuver_Crater, 
			"Skynd Crater" => main.Skynd_Crater, 
			"Telemachus" => main.Telemachus, 
			"Ithaca Chasma" => main.Ithaca_Chasma, 
			"Palatine Chasmata" => main.Palatine_Chasmata, 
			"Evander Crater" => main.Evander_Crater, 
			"Tirawa Crater" => main.Tirawa_Crater, 
			"Laica Crater" => main.Laica_Crater, 
			"Kachina Chasmata" => main.Kachina_Chasmata, 
			"Urvara Crater" => main.Urvara_Crater, 
			"Kerwan Crater" => main.Kerwan_Crater, 
			"Bosphorus Regio" => main.Bosphorus_Regio, 
			"Colchis Regio" => main.Colchis_Regio, 
			"Chalybes Regio" => main.Chalybes_Regio, 
			"Conamara Chaos" => main.Conamara_Chaos, 
			"Galileo Regio" => main.Galileo_Regio, 
			"Equatorial Ridge" => main.Equatorial_Ridge, 
			"Tiger Stripes" => main.Tiger_Stripes, 
			"Senkyo Ocean" => main.Senkyo_Ocean, 
			"Fensal Ocean" => main.Fensal_Ocean, 
			"Shangri La Ocean" => main.Shangri_La_Ocean, 
			"Bogle Crater" => main.Bogle_Crater, 
			"Lob Crater" => main.Lob_Crater, 
			"Butz Crater" => main.Butz_Crater, 
			"Bona Chasma" => main.Bona_Chasma, 
			"Pharos Crater" => main.Pharos_Crater, 
			"Guttae" => main.Guttae, 
			"Herschel Crater" => main.Herschel_Crater, 
			"Sputnik Planitia" => main.Sputnik_Planitia, 
			"Serenity Chasma" => main.Serenity_Chasma, 
			"Mordor Macula" => main.Mordor_Macula, 
			"Metztli Crater" => main.Metztli_Crater, 
			"Zethus Crater" => main.Zethus_Crater, 
			_ => MissingTranslation(), 
		};
		Field MissingTranslation()
		{
			if (!Application.isEditor)
			{
				return Field.Text(codeName);
			}
			Debug.LogError("Missing landmark translation for '" + codeName + "'");
			if (MsgDrawer.main != null)
			{
				MsgDrawer.main.Log("Missing landmark translation for '" + codeName + "'");
			}
			return Field.Text("*" + codeName);
		}
	}
}
