using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
[CreateAssetMenu(fileName = "FB_NewDescription", menuName = "Pre&Post-Test/Fill in the Blank Description")]
public class FillInBlankDescription : BaseDescription
{
    public FillInBlankQuestion FillInBlank;
}