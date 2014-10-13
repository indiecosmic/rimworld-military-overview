using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using System.Linq;
using VerseBase;


namespace MilitaryOverview
{
    public class OTab_Military : OverviewTab
    {
        private const float TopAreaHeight = 40f;
        private const float LabelRowHeight = 50f;
        private const float PawnRowHeight = 30f;
        private const float NameColumnWidth = 175f;
        private const float NameLeftMargin = 15f;
        private const float StrikethroughY = 15f;
        private Vector2 scrollPosition = Vector2.zero;
        private static readonly Color DisabledSkillColor = new Color(1f, 1f, 1f, 0.5f);
        private static Texture2D SkillBarFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.1f));
        private static Texture2D PassionMinorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMinor", true);
        private static Texture2D PassionMajorIcon = ContentFinder<Texture2D>.Get("UI/Icons/PassionMajor", true);

        public OTab_Military()
        {
            this.title = "Military";
            this.order = 999;
        }

        public override void PanelOnGUI(Rect fillRect)
        {
            Rect innerRect = fillRect.GetInnerRect(10f);
            GUI.BeginGroup(innerRect);
            Rect position = new Rect(0f, 0f, innerRect.width, innerRect.height);
            GUI.BeginGroup(position);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            Rect outRect = new Rect(0f, 50f, position.width, position.height - 50f);

            float listHeight = (float)Find.ListerPawns.FreeColonistsCount * 30f;
            Rect rect = new Rect(0f, 0f, position.width - 16f, listHeight);
            this.scrollPosition = Widgets.BeginScrollView(outRect, this.scrollPosition, rect);

            float offsetY = 0f;
            foreach (Pawn currentPawn in Find.ListerPawns.FreeColonists)
            {
                GUI.color = new Color(1f, 1f, 1f, 0.2f);
                Widgets.DrawLineHorizontal(new Vector2(0f, offsetY), rect.width);
                GUI.color = Color.white;
                this.DrawPawnRow(currentPawn, offsetY, rect);
                offsetY += 30f;
            }

            Widgets.EndScrollView();
            GUI.EndGroup();
            GUI.EndGroup();
        }

        private void DrawPawnRow(Pawn pawn, float rowY, Rect fillRect)
        {
            Rect position = new Rect(0f, rowY, fillRect.width, 30f);
            if (position.Contains(Event.current.mousePosition))
            {
                GUI.DrawTexture(position, TexUI.HighlightTex);
            }
            Rect rect = new Rect(0f, rowY, 175f, 30f);
            Rect innerRect = rect.GetInnerRect(3f);
            if (pawn.healthTracker.Health < pawn.healthTracker.MaxHealth - 1)
            {
                Rect screenRect = new Rect(rect);
                screenRect.xMin -= 4f;
                screenRect.yMin += 4f;
                screenRect.yMax -= 6f;
                Widgets.FillableBar(screenRect, (float)pawn.healthTracker.Health / (float)pawn.healthTracker.MaxHealth, PawnUIOverlay.HealthTex, false, BaseContent.ClearTex);
            }
            if (rect.Contains(Event.current.mousePosition))
            {
                GUI.DrawTexture(innerRect, TexUI.HighlightTex);
            }
            Text.Font = GameFont.Small;
            Text.Alignment = TextAnchor.MiddleLeft;
            Rect rect2 = new Rect(rect);
            rect2.xMin += 15f;
            Widgets.Label(rect2, pawn.Label);
            if (Widgets.InvisibleButton(rect))
            {
                Find.LayerStack.FirstLayerOfType<Dialog_Overview>().Close();
                Find.CameraMap.JumpTo(pawn.Position);
                return;
            }
            TipSignal tooltip = pawn.GetTooltip();
            tooltip.text = "ClickToJumpTo".Translate() + "\n\n" + tooltip.text;
            TooltipHandler.TipRegion(rect, tooltip);

            SkillRecord skill = pawn.skills.GetSkill(SkillDefOf.Shooting);
            float skillBarLeftMargin = 175f;

            Rect passionIconPosition = new Rect(skillBarLeftMargin, rowY + 2.5f, 24f, 24f);
            if (skill.passion > Passion.None)
            {
                Texture2D image = (skill.passion != Passion.Major) ? PassionMinorIcon : PassionMajorIcon;
                GUI.DrawTexture(passionIconPosition, image);
            }

            skillBarLeftMargin += 24f;
            Vector2 topLeft = new Vector2(skillBarLeftMargin, rowY + 2.5f);


            if (!skill.TotallyDisabled)
            {
                Widgets.FillableBar(new Rect(topLeft.x, topLeft.y, 240f, 24f), (float)skill.level / 20f, SkillBarFillTex, false, null);
            }

            string label;
            if (skill.TotallyDisabled)
            {
                GUI.color = DisabledSkillColor;
                label = "-";

            }
            else
            {
                label = GenString.NumberString(skill.level);
            }
            Text.Alignment = TextAnchor.MiddleLeft;
            Widgets.Label(new Rect(topLeft.x + 4f, topLeft.y, 240f, 24f), label);

            if (pawn.Downed)
            {
                GUI.color = Color.red;
                Widgets.DrawLineHorizontal(new Vector2(175f, rowY + 15f), skillBarLeftMargin - 175f - 17f);
            }
            GUI.color = Color.white;


            // Draw equipment
            if (pawn.equipment != null)
            {
                Equipment weapon = pawn.equipment.AllEquipment.FirstOrDefault();
                if (weapon != null)
                {
                    // Draw weapon
                    Rect weaponRect = new Rect(topLeft.x + 240f + 4f, topLeft.y, 30f, 24f);
                    Widgets.ThingIcon(weaponRect, weapon);
                    Widgets.Label(new Rect(weaponRect.max.x + 4f, topLeft.y, 100f, 24f), new GUIContent(weapon.LabelCap));
                }
            }

            // Draw hit chance factor
            float f = 1f;
            float statValue = pawn.GetStatValue(StatDefOf.HitChanceFactor, true);
            float statValue2 = pawn.GetStatValue(StatDefOf.ShootingAccuracy, true);
            if (statValue < 0f)
            {
                f = statValue2 + statValue * statValue2;
            }
            else
            {
                f = statValue2 + statValue * (1f - statValue2);
            }

            Rect hitChanceRect = new Rect(topLeft.x + 240f + 4f + 30f + 100f, topLeft.y, 50f, 24f);
            Widgets.Label(hitChanceRect, GenText.AsPercent(f));

            // Draw sight efficiency
            Widgets.Label(new Rect(hitChanceRect.max.x + 4f, hitChanceRect.y, 50f, 24f), GenText.AsPercent(pawn.healthTracker.GetEfficiency(PawnActivityDefOf.Sight)));
        }
    }
}
