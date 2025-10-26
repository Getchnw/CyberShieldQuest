using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lock_comingsoon : MonoBehaviour
{
    [Header("Children Object in Box")]
    [Tooltip("Assign the parent box objects (e.g. Story1, Story2, ...) to check their children for the 'ComingSoon' tag.")]
    [SerializeField] private GameObject[] Member_in_Box;

    void Start()
    {
        RefreshLocks();
    }

    // You can call this from code or from the inspector (Context Menu) to re-evaluate locks
    [ContextMenu("Refresh Locks")]
    public void RefreshLocks()
    {
        if (Member_in_Box == null || Member_in_Box.Length == 0) return;

        foreach (var member in Member_in_Box)
        {
            if (member == null) continue;

            bool hasComingSoon = false;

            // Check the member itself
            if (member.CompareTag("ComingSoon"))
            {
                hasComingSoon = true;
            }
            else
            {
                // Check all children (include inactive)
                var children = member.GetComponentsInChildren<Transform>(true);
                foreach (var t in children)
                {
                    if (t == null) continue;
                    if (t.gameObject.CompareTag("ComingSoon"))
                    {
                        hasComingSoon = true;
                        break;
                    }
                }
            }

            if (hasComingSoon)
            {
                // Try to find a Button on the member or its children and disable it
                var btn = member.GetComponentInChildren<Button>(true);
                if (btn != null)
                {
                    btn.interactable = false;
                }
                else
                {
                    // Fallback: disable any Button components under this member
                    var buttons = member.GetComponentsInChildren<Button>(true);
                    foreach (var b in buttons)
                    {
                        if (b != null) b.interactable = false;
                    }
                }
            }
        }
    }
}
