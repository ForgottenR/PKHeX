using SkiaSharp;
using PKHeX.Core;
using PKHeX.Drawing.PokeSprite.Properties;

namespace PKHeX.Drawing.PokeSprite;

/// <summary>
/// 56 high, 68 wide sprite builder using Artwork Sprites
/// </summary>
public sealed class SpriteBuilder5668a : SpriteBuilder
{
    public override int Height => 56;
    public override int Width => 68;

    protected override int ItemShiftX => 2;
    protected override int ItemShiftY => 2;
    protected override int ItemMaxSize => 32;
    protected override int EggItemShiftX => 18;
    protected override int EggItemShiftY => 1;
    public override bool HasFallbackMethod => true;

    protected override string GetSpriteStringSpeciesOnly(ushort species) => 'a' + $"_{species}";
    protected override string GetSpriteAll(ushort species, byte form, byte gender, uint formarg, bool shiny, EntityContext context) => 'a' + SpriteName.GetResourceStringSprite(species, form, gender, formarg, context, shiny);
    protected override string GetSpriteAllSecondary(ushort species, byte form, byte gender, uint formarg, bool shiny, EntityContext context) => 'b' + SpriteName.GetResourceStringSprite(species, form, gender, formarg, context, shiny);
    protected override string GetItemResourceName(int item) => 'a' + $"item_{item}";
    protected override SKBitmap Unknown => ImageUtil.GetSKBitmap(Resources.b_unknown);
    protected override SKBitmap GetEggSprite(ushort species) => species == (int)Species.Manaphy ? ImageUtil.GetSKBitmap(Resources.a_490_e) : ImageUtil.GetSKBitmap(Resources.a_egg);

    public override SKBitmap Hover => ImageUtil.GetSKBitmap(Resources.slotHover68);
    public override SKBitmap View => ImageUtil.GetSKBitmap(Resources.slotView68);
    public override SKBitmap Set => ImageUtil.GetSKBitmap(Resources.slotSet68);
    public override SKBitmap Delete => ImageUtil.GetSKBitmap(Resources.slotDel68);
    public override SKBitmap Transparent => ImageUtil.GetSKBitmap(Resources.slotTrans68);
    public override SKBitmap Drag => ImageUtil.GetSKBitmap(Resources.slotDrag68);
    public override SKBitmap UnknownItem => ImageUtil.GetSKBitmap(Resources.bitem_unk);
    public override SKBitmap None => ImageUtil.GetSKBitmap(Resources.b_0);
    public override SKBitmap ItemTM => ImageUtil.GetSKBitmap(Resources.aitem_tm);
    public override SKBitmap ItemTR => ImageUtil.GetSKBitmap(Resources.bitem_tr);
    public override SKBitmap ShadowLugia => ImageUtil.GetSKBitmap(Resources.b_249x);
}
