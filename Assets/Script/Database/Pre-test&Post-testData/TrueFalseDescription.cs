using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
[CreateAssetMenu(fileName = "TF_NewDescription", menuName = "Pre&Post-Test/True-False Description")]
public class TrueFalseDescription : BaseDescription
{
    public TrueFalseQuestion TrueFalse;
}