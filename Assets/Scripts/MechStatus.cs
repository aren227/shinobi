using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Mech Status")]
public class MechStatus : ScriptableObject
{
    public int bodyArmorHealth;
    public int bodyFrameHealth;
    public int headArmorHealth;
    public int headFrameHealth;
    public int partsArmorHealth;
    public int partsFrameHealth;
    public int thrusterHealth;

    public void Initialize(Mech mech) {
        Part body = mech.skeleton.GetPart(PartName.BODY);
        body.armorDurability = bodyArmorHealth;
        body.frameDurability = bodyFrameHealth;
        body.health = bodyArmorHealth + bodyFrameHealth;

        Part head = mech.skeleton.GetPart(PartName.HEAD);
        head.armorDurability = headArmorHealth;
        head.frameDurability = headFrameHealth;
        head.health = headArmorHealth + headFrameHealth;

        PartName[] partNames = new PartName[] {
            PartName.UPPER_LEFT_ARM, PartName.LOWER_LEFT_ARM,
            PartName.UPPER_RIGHT_ARM, PartName.LOWER_RIGHT_ARM,
            PartName.UPPER_LEFT_LEG, PartName.LOWER_LEFT_LEG,
            PartName.UPPER_RIGHT_LEG, PartName.LOWER_RIGHT_LEG,
        };

        foreach (PartName partName in partNames) {
            Part part = mech.skeleton.GetPart(partName);
            part.armorDurability = partsArmorHealth;
            part.frameDurability = partsFrameHealth;
            part.health = partsArmorHealth + partsFrameHealth;
        }

        mech.skeleton.thruster.damagable.maxHealth = thrusterHealth;
        mech.skeleton.thruster.damagable.health = thrusterHealth;
    }
}