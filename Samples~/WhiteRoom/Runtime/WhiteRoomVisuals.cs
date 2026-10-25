using UnityEngine;

namespace Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom
{
    public static class WhiteRoomVisuals
    {
        public static GameObject CreateRobotModel(Transform parent, out Transform rightFist)
        {
            GameObject root = new GameObject("VisualRoot");
            root.transform.SetParent(parent, false);

            Material whiteMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse"));
            whiteMat.color = new Color(0.92f, 0.92f, 0.92f);

            Material darkMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse"));
            darkMat.color = new Color(0.2f, 0.22f, 0.25f);

            Material visorMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse"));
            visorMat.color = new Color(0.1f, 0.7f, 1.0f);

            // Torso
            GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
            torso.name = "Torso";
            torso.transform.SetParent(root.transform, false);
            torso.transform.localPosition = new Vector3(0, 1.15f, 0);
            torso.transform.localScale = new Vector3(0.55f, 0.7f, 0.35f);
            torso.GetComponent<Renderer>().sharedMaterial = whiteMat;
            Object.DestroyImmediate(torso.GetComponent<Collider>());

            // Head
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "Head";
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0, 1.7f, 0);
            head.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            head.GetComponent<Renderer>().sharedMaterial = whiteMat;
            Object.DestroyImmediate(head.GetComponent<Collider>());

            // Visor (Orientation indicator)
            GameObject visor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visor.name = "Visor";
            visor.transform.SetParent(head.transform, false);
            visor.transform.localPosition = new Vector3(0, 0.05f, 0.52f);
            visor.transform.localScale = new Vector3(0.85f, 0.25f, 0.15f);
            visor.GetComponent<Renderer>().sharedMaterial = visorMat;
            Object.DestroyImmediate(visor.GetComponent<Collider>());

            // Left Arm
            GameObject leftArm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftArm.name = "LeftArm";
            leftArm.transform.SetParent(root.transform, false);
            leftArm.transform.localPosition = new Vector3(-0.4f, 1.1f, 0);
            leftArm.transform.localScale = new Vector3(0.18f, 0.6f, 0.18f);
            leftArm.GetComponent<Renderer>().sharedMaterial = darkMat;
            Object.DestroyImmediate(leftArm.GetComponent<Collider>());

            // Right Arm (Fist)
            GameObject rightArmRoot = new GameObject("RightArmRoot");
            rightArmRoot.transform.SetParent(root.transform, false);
            rightArmRoot.transform.localPosition = new Vector3(0.4f, 1.1f, 0);

            GameObject rightArm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightArm.name = "RightArm";
            rightArm.transform.SetParent(rightArmRoot.transform, false);
            rightArm.transform.localPosition = new Vector3(0, 0, 0);
            rightArm.transform.localScale = new Vector3(0.18f, 0.6f, 0.18f);
            rightArm.GetComponent<Renderer>().sharedMaterial = darkMat;
            Object.DestroyImmediate(rightArm.GetComponent<Collider>());

            rightFist = rightArmRoot.transform;

            // Hips & Legs
            GameObject leftLeg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftLeg.name = "LeftLeg";
            leftLeg.transform.SetParent(root.transform, false);
            leftLeg.transform.localPosition = new Vector3(-0.18f, 0.45f, 0);
            leftLeg.transform.localScale = new Vector3(0.2f, 0.8f, 0.22f);
            leftLeg.GetComponent<Renderer>().sharedMaterial = darkMat;
            Object.DestroyImmediate(leftLeg.GetComponent<Collider>());

            GameObject rightLeg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightLeg.name = "RightLeg";
            rightLeg.transform.SetParent(root.transform, false);
            rightLeg.transform.localPosition = new Vector3(0.18f, 0.45f, 0);
            rightLeg.transform.localScale = new Vector3(0.2f, 0.8f, 0.22f);
            rightLeg.GetComponent<Renderer>().sharedMaterial = darkMat;
            Object.DestroyImmediate(rightLeg.GetComponent<Collider>());

            return root;
        }
    }
}
