using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveScores {

	public static void SaveDataToCSV(int[] gens, float[] bestScores, float[] averageScores)
    {
        string filePath = getPath();

        if (!File.Exists(filePath))
        {
            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(filePath))
            {
                sw.WriteLine("Gen,BestScore,AverageScore");
            }
        }

        // This text is always added, making the file longer over time
        // if it is not deleted.
        using (StreamWriter sw = File.AppendText(filePath))
        {
            string line;
            for (int i = 0; i < gens.Length; i++)
            {
                line = gens[i] + "," + bestScores[i] + "," + averageScores[i];
                sw.WriteLine(line);
            }

        }


    }

    public static void deleteCSV()
    {
        string filePath = getPath();
        File.Delete(filePath);
    }


    private static string getPath()
    {
        #if UNITY_EDITOR
                return Application.dataPath + "/CSV/" + "Saved_data.csv";
        #elif UNITY_ANDROID
                return Application.persistentDataPath+"Saved_data.csv";
        #elif UNITY_IPHONE
                return Application.persistentDataPath+"/"+"Saved_data.csv";
        #else
                return Application.dataPath +"/"+"Saved_data.csv";
        #endif
    }
}
