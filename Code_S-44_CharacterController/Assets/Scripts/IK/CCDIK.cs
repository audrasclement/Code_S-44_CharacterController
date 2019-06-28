using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CCDIK algorithm to animate the character's leg procedurally.
/// cf. Unite Berlin 2018 - An Introduction to CCD IK and How to use it
/// </summary>

[HelpURL("https://www.youtube.com/watch?v=MA1nT9RAF3k")]
public class CCDIK : MonoBehaviour
{

    public Transform goal;
    public Transform effector;
    public Transform baseBone;
    public float sqrDistError = 0.01f;

    [Range(0, 1)]
    public float weight = 1;

    public int MaxIterationCount = 10;

    public List<Transform> m_Bones;

    // Update is called once per fixed framerate frame (0.2)
    void LateUpdate()
    {
        Solve();
    }

    void Solve()
    {
        Vector3 goalPosition = goal.position;
        Vector3 effectorPosition = m_Bones[0].position;

        Vector3 targetPosition = Vector3.Lerp(effectorPosition, goalPosition, weight);
        float sqrDistance;

        int iterationCount = 0;
        do
        {
            for (int i = 0; i < m_Bones.Count - 2; i++)
            {
                for (int j = 1; j < m_Bones.Count; j++)
                {
                    RotateBone(m_Bones[0], m_Bones[j], targetPosition);

                    sqrDistance = (m_Bones[0].position - targetPosition).sqrMagnitude;

                    if (sqrDistance <= sqrDistError)
                        return;
                }
            }

            sqrDistance = (m_Bones[0].position - targetPosition).sqrMagnitude;
            iterationCount++;
        }
        while (sqrDistance > sqrDistError && iterationCount <= MaxIterationCount);
    }


    public static void RotateBone(Transform effector, Transform bone, Vector3 goalPosition)
    {
        Vector3 effectorPosition = effector.position;
        Vector3 bonePosition = bone.position;
        Quaternion boneRotation = bone.rotation;

        Vector3 boneToEffector = effectorPosition - bonePosition;
        Vector3 boneToGoal = goalPosition - bonePosition;

        Quaternion fromToRotation = Quaternion.FromToRotation(boneToEffector, boneToGoal);
        Quaternion newRotation = fromToRotation * boneRotation;

        bone.rotation = newRotation;
    }
}
