
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


public static class numericUtils 
{
    static Dictionary<int,string> numberSuffix = new Dictionary<int, string>
    {
        {1,"K"}, 
        {2,"M"},
        {3,"B"}, 
        {4,"T"},
        {5,"Q"},
        {6,"X"},
        {7,"?"}
    };

  
   public static string numberShortener(float number)
    {
        string Prefix ="";
        if(number<0)
        {

            Debug.LogWarning("number is negantive!!!!!!");
            Prefix = "-";
        }
        if(number== 0)// need to have this because log of zero is undefined
        {
            return "0";
        }
        float absNum = math.abs(number);
        int digits = (int)math.log10(absNum) + 1;
        if(digits <= 3)
        {
            return math.round(absNum).ToString();
        }      
        int suffixIndice = (digits-1) /3;
        float shortenedNumber = math.round(absNum /Mathf.Pow(10,suffixIndice));
        
        if(suffixIndice>numberSuffix.Count)
        {
            Debug.LogWarning("number way too large! number has " + digits +" digits!");
            return "Big";
        }
        string suffix =numberSuffix[(int)suffixIndice];
        double superShortenedNumber = Math.Round(shortenedNumber/Mathf.Pow(100,suffixIndice),2);

        string completedNumber = Prefix+superShortenedNumber+suffix;
        return completedNumber;
        
        
        

    }
}
