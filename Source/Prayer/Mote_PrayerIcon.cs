namespace EnhancedIdeology;

internal sealed class Mote_PrayerIcon : Mote
{
    private Material? iconMat;
    private Vector3 velocity;

    internal void Setup(Texture2D icon, Color iconColor)
    {
        iconMat = MaterialPool.MatFrom(icon, ShaderDatabase.TransparentPostLight, iconColor);
        rotationRate = Rand.Range(-3f, 3f);
        float angle = Rand.Range(30f, 60f);
        velocity = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, Mathf.Sin(angle * Mathf.Deg2Rad)) * 0.42f;
    }

    protected override void TimeInterval(float deltaTime)
    {
        base.TimeInterval(deltaTime);
        if (!Destroyed)
        {
            exactPosition += velocity * deltaTime;
            exactRotation += rotationRate * deltaTime;
        }
    }

    private const float MaxAlpha = 0.6f;

    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        if (iconMat == null || Find.UIRoot.HideMotes || paused)
            return;
        float alpha = Alpha * MaxAlpha;
        if (alpha <= 0f)
            return;

        var mat = MaterialPool.MatFrom((Texture2D)iconMat.mainTexture, iconMat.shader,
            iconMat.color.WithAlpha(alpha));

        var drawPos = drawLoc;
        drawPos.y = def.altitudeLayer.AltitudeFor() + yOffset;

        var matrix = Matrix4x4.TRS(drawPos, Quaternion.AngleAxis(exactRotation, Vector3.up), new Vector3(0.5f, 1f, 0.5f));
        Graphics.DrawMesh(MeshPool.plane10, matrix, mat, 0);
    }
}
