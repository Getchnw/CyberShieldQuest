using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
[CreateAssetMenu(fileName = "M_NewDescription", menuName = "Pre&Post-Test/Matching Description")]
public class MatchingDescription : BaseDescription
{
    public MatchingQuestion Matching;
    public string Question_of_Matching;
}