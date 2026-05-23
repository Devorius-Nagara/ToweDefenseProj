"""
gen_sprites2.py  –  High-quality cartoon goblin sprites for Tower Defense.

Images the user provided:
  Image 1 (green goblin with axe)     → green-goblin.png   (Goblin  – weakest)
  Image 4 (green goblin with club)    → blue-goblin.png    (Orc     – medium)
  Image 2 (red demon with shield/axe) → red-goblin.png     (Ghost   – strongest)
  Image 3 (orange explosion burst)    → goblin-hit.png     (hit FX)
"""
from PIL import Image, ImageDraw, ImageFilter
import math, os, sys

OUT = r"D:\Projects\Unity\ToweDefenseProj\Assets\Resources\Sprites"
os.makedirs(OUT, exist_ok=True)

def lerp_color(c1, c2, t):
    return tuple(int(a + (b - a) * t) for a, b in zip(c1, c2))

# ── green-goblin.png  (128×128, green goblin with axe) ───────────────────────
def make_green_goblin(W=128, H=128):
    img = Image.new("RGBA", (W, H), (0,0,0,0))
    d   = ImageDraw.Draw(img)

    # palette
    G      = (85,140,50,255)
    DG     = (58,100,35,255)
    BROWN  = (105,68,30,255)
    DBROWN = (70,44,18,255)
    TAN    = (175,128,62,255)
    METAL  = (148,148,152,255)
    DMETAL = (88,88,94,255)
    WHITE  = (242,238,224,255)
    YELL   = (218,190,58,255)
    BLK    = (28,20,10,255)
    RED    = (190,40,30,255)

    def e(x1,y1,x2,y2,fill):  d.ellipse([x1,y1,x2,y2], fill=fill)
    def r(x1,y1,x2,y2,fill):  d.rectangle([x1,y1,x2,y2], fill=fill)
    def p(pts, fill):          d.polygon(pts, fill=fill)

    # --- feet / boots
    e(42,110,66,124, DBROWN)
    e(62,110,86,124, DBROWN)
    # --- legs
    e(45,97,64,115,  BROWN)
    e(63,97,82,115,  BROWN)
    # --- body
    e(40,66,88,106,  BROWN)
    r(44,78,84,106,  BROWN)
    # --- arms
    e(28,68,52,90,   G)
    e(76,65,102,88,  G)
    # --- head
    e(32,28,96,86,   G)
    # --- ears
    e(24,42,44,64,   G)
    e(84,42,104,64,  G)
    e(28,46,40,60,   DG)
    e(88,46,100,60,  DG)
    # --- leather cap (brown)
    e(30,20,98,55,   DBROWN)
    r(30,38,98,55,   DBROWN)
    r(26,46,102,56,  BROWN)   # rim
    # stitches on cap
    for i in range(4):
        x = 40 + i*15
        r(x,25,x+5,30, TAN)
    # --- eyes
    e(44,48,60,62,   YELL)
    e(66,48,82,62,   YELL)
    e(48,51,58,60,   BLK)
    e(70,51,80,60,   BLK)
    e(51,52,55,56,   WHITE)
    e(73,52,77,56,   WHITE)
    # --- nose
    e(56,60,72,70,   DG)
    e(59,62,65,68,   (50,88,30,255))
    e(65,62,71,68,   (50,88,30,255))
    # --- frown
    for i in range(9):
        x = int(48 + i*3.2)
        y = int(72 + abs(i-4)*0.9)
        r(x,y,x+2,y+2, BLK)
    # spots on face
    for (sx,sy) in [(42,58),(68,64),(59,42),(76,55)]:
        e(sx-2,sy-2,sx+2,sy+2, DG)
    # --- axe (right hand)
    r(88,60,94,92,   DBROWN)     # handle
    p([(86,60),(108,50),(112,70),(92,68)], METAL)  # blade
    p([(88,62),(106,54),(109,68),(93,66)], (175,175,180,255))  # shine
    # small guard
    r(85,67,95,72,   DMETAL)

    img = img.filter(ImageFilter.SMOOTH_MORE)
    return img

# ── blue-goblin.png  (148×148, stockier green goblin with club) ───────────────
def make_blue_goblin(W=148, H=148):
    img = Image.new("RGBA", (W, H), (0,0,0,0))
    d   = ImageDraw.Draw(img)

    G      = (78,148,55,255)
    DG     = (52,108,38,255)
    GRAY   = (148,144,138,255)
    DGRAY  = (96,94,90,255)
    YELL   = (215,182,48,255)
    DYELL  = (155,128,28,255)
    RB     = (118,50,28,255)
    DRB    = (80,34,18,255)
    WHITE  = (242,238,224,255)
    OY     = (222,188,58,255)
    BLK    = (24,18,12,255)

    def e(x1,y1,x2,y2,fill):  d.ellipse([x1,y1,x2,y2], fill=fill)
    def r(x1,y1,x2,y2,fill):  d.rectangle([x1,y1,x2,y2], fill=fill)
    def p(pts, fill):          d.polygon(pts, fill=fill)

    # boots
    e(36,122,64,146, DRB)
    e(84,122,112,146, DRB)
    r(36,128,62,146,  DRB)
    r(84,128,110,146, DRB)
    e(32,136,58,148,  RB)
    e(90,136,116,148, RB)
    # legs
    e(42,108,68,136,  DYELL)
    e(80,108,106,136, DYELL)
    # body
    e(42,74,106,122,  GRAY)
    r(46,90,102,122,  GRAY)
    # belt
    r(44,106,104,114, DRB)
    r(68,104,80,116,  (185,152,52,255))
    # arms
    e(22,76,54,104,   G)
    e(94,74,126,102,  G)
    e(18,94,44,114,   G)
    e(104,92,130,112, G)
    # head (bigger, wide)
    e(34,28,114,90,   G)
    # big pointy ears
    p([(26,52),(14,28),(46,44)], G)
    p([(122,52),(134,28),(102,44)], G)
    p([(28,50),(18,32),(44,46)], DG)
    p([(120,50),(130,32),(104,46)], DG)
    # eyes (angry slant)
    e(48,50,68,64,    OY)
    e(80,50,100,64,   OY)
    e(52,53,66,62,    BLK)
    e(84,53,98,62,    BLK)
    e(55,54,59,58,    WHITE)
    e(87,54,91,58,    WHITE)
    # brow
    for i in range(7): r(48+i*2,44-i//3,50+i*2,47-i//3, DG)
    for i in range(7): r(80+i*2,42+i//3,82+i*2,45+i//3, DG)
    # nose
    e(64,62,84,74,    DG)
    e(66,64,72,70,    (48,88,32,255))
    e(72,64,78,70,    (48,88,32,255))
    # mouth + fang
    r(54,76,94,83,    BLK)
    r(69,76,76,86,    WHITE)
    r(78,76,83,84,    WHITE)
    # club (spiked)
    r(106,60,116,104, DRB)
    e(102,36,126,72,  DGRAY)
    e(104,38,124,70,  GRAY)
    for ang in range(0,360,50):
        rad = math.radians(ang)
        cx2,cy2 = 114,54
        x1=int(cx2+10*math.cos(rad)); y1=int(cy2+10*math.sin(rad))
        x2=int(cx2+17*math.cos(rad)); y2=int(cy2+17*math.sin(rad))
        d.line([(x1,y1),(x2,y2)], fill=DGRAY, width=3)
        e(x2-3,y2-3,x2+3,y2+3, DGRAY)

    img = img.filter(ImageFilter.SMOOTH_MORE)
    return img

# ── red-goblin.png  (200×200, large red demon with shield & axe) ─────────────
def make_red_goblin(W=200, H=200):
    img = Image.new("RGBA", (W, H), (0,0,0,0))
    d   = ImageDraw.Draw(img)

    RED    = (202,54,38,255)
    DRED   = (148,32,22,255)
    VDRED  = (96,18,12,255)
    TAN    = (195,154,104,255)
    DTAN   = (152,112,68,255)
    AD     = (72,62,52,255)    # armor dark
    AM     = (98,86,70,255)    # armor mid
    AL     = (128,114,92,255)  # armor light
    WD     = (138,88,42,255)   # wood
    DWD    = (98,60,26,255)    # dark wood
    MTL    = (148,146,152,255)
    DMTL   = (94,92,98,255)
    YELL   = (228,198,48,255)
    BLK    = (18,14,8,255)
    WHITE  = (242,238,226,255)
    PINK   = (185,58,118,255)
    LPINK  = (222,98,158,255)

    def e(x1,y1,x2,y2,fill,out=None,ow=0):
        if out: d.ellipse([x1,y1,x2,y2], fill=fill, outline=out, width=ow)
        else:   d.ellipse([x1,y1,x2,y2], fill=fill)
    def r(x1,y1,x2,y2,fill):  d.rectangle([x1,y1,x2,y2], fill=fill)
    def p(pts, fill):          d.polygon(pts, fill=fill)

    # === Legs ===
    e(56,130,90,166,  DRED)
    e(110,130,144,166, DRED)
    e(52,152,86,186,  VDRED)
    e(114,152,148,186, VDRED)
    e(42,170,82,192,  VDRED)
    e(118,170,158,192, VDRED)
    e(38,180,66,196,  DRED)
    e(134,180,162,196, DRED)

    # === Body ===
    e(50,84,150,144,  AD)
    r(54,102,146,144, AD)
    # belt
    r(52,128,148,138, DWD)
    r(90,126,110,140, (188,152,58,255))  # buckle
    # chest gem
    e(86,106,114,130, PINK)
    e(90,110,110,126, LPINK)
    e(94,114,106,122, WHITE)

    # === Shoulder pauldrons ===
    e(32,82,78,118,   AM)
    e(122,82,168,118, AM)
    e(36,84,74,114,   AL)
    e(126,84,164,114, AL)
    for (sx,sy) in [(50,90),(56,104),(148,90),(142,104)]:
        e(sx-4,sy-4,sx+4,sy+4, MTL)

    # === Hood / Cowl ===
    p([(58,42),(100,2),(142,42),(148,76),(52,76)], TAN)
    d.line([(100,2),(100,76)], fill=DTAN, width=4)
    for i in range(4):
        y = 15+i*14
        d.line([(92,y),(108,y+4)], fill=DTAN, width=2)
    e(56,52,144,84,   DTAN)

    # === Head ===
    e(56,48,144,96,   RED)
    # horns
    p([(60,58),(48,36),(74,54)], DRED)
    p([(140,58),(152,36),(126,54)], DRED)

    # === Eyes ===
    e(66,60,90,76,    YELL)
    e(110,60,134,76,  YELL)
    e(71,63,87,73,    BLK)
    e(113,63,129,73,  BLK)
    e(74,64,79,69,    WHITE)
    e(116,64,121,69,  WHITE)

    # === Nose ===
    e(86,72,114,82,   DRED)
    e(88,74,96,80,    VDRED)
    e(96,74,104,80,   VDRED)

    # === Mouth / Fangs ===
    r(72,84,128,92,   BLK)
    p([(80,84),(86,84),(83,96)], WHITE)
    p([(110,84),(116,84),(113,96)], WHITE)

    # === Left arm + shield ===
    e(12,90,58,130,   RED)
    e(6,108,46,154,   DRED)
    # round wooden shield
    e(-6,82,56,158,   WD, out=(0,0,0,0), ow=0)
    e(-2,86,52,154,   (158,104,48,255))
    for i in range(5):
        y = 96 + i*12
        d.arc([-2,y-8,52,y+8], 0, 180, fill=DWD, width=2)
    e(-6,82,56,158,   (0,0,0,0), out=MTL, ow=4)
    # boss
    e(14,110,38,130,  MTL)
    e(18,114,34,126,  (178,176,182,255))

    # === Right arm + axe ===
    e(142,90,186,130, RED)
    e(152,112,192,154, DRED)
    # handle
    r(160,86,172,164, DWD)
    e(158,80,174,96,  WD)
    e(158,158,174,174, WD)
    # axe blade (large)
    p([(162,90),(200,68),(204,112),(186,132),(164,120)], MTL)
    p([(164,94),(196,74),(200,110),(182,126),(166,116)], (176,174,180,255))
    for (ax,ay) in [(172,98),(178,110)]:
        e(ax-4,ay-4,ax+4,ay+4, DMTL)

    img = img.filter(ImageFilter.SMOOTH_MORE)
    return img

# ── goblin-hit.png  (128×128, orange/yellow explosion starburst) ─────────────
def make_hit_effect(W=128, H=128):
    img = Image.new("RGBA", (W, H), (0,0,0,0))
    d   = ImageDraw.Draw(img)

    cx, cy = W//2, H//2

    def starburst(n, r_out, r_in, col, offset_deg=0):
        pts = []
        for i in range(n*2):
            ang = math.radians(i * 180/n + offset_deg - 90)
            r   = r_out if i%2==0 else r_in
            pts.append((cx + r*math.cos(ang), cy + r*math.sin(ang)))
        d.polygon(pts, fill=col)

    # layers: dark→mid→light→core
    starburst(14, W*0.46, W*0.20, (180,70,8,255),   0)
    starburst(12, W*0.39, W*0.18, (228,108,18,255),  8)
    starburst(10, W*0.32, W*0.15, (252,172,36,255), 16)
    starburst( 8, W*0.24, W*0.12, (255,224,72,255), 24)

    # glowing core
    d.ellipse([cx-W*0.17,cy-W*0.17,cx+W*0.17,cy+W*0.17], fill=(255,240,120,255))
    d.ellipse([cx-W*0.10,cy-W*0.10,cx+W*0.10,cy+W*0.10], fill=(255,255,210,255))

    img = img.filter(ImageFilter.SMOOTH)
    return img

# ── meta file helper ──────────────────────────────────────────────────────────
META_TEMPLATE = """\
fileFormatVersion: 2
guid: {guid}
TextureImporter:
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsPerUnit: 100
  textureType: 8
  filterMode: 2
  maxTextureSize: 2048
  compressionQuality: 50
  userData:
  assetBundleName:
  assetBundleVariant:
"""

import hashlib

def write_meta(path):
    g = hashlib.md5(os.path.basename(path).encode()).hexdigest()
    with open(path + ".meta", "w") as f:
        f.write(META_TEMPLATE.replace("{guid}", g))

# ── Save all ──────────────────────────────────────────────────────────────────
sprites = [
    ("green-goblin.png", make_green_goblin()),
    ("blue-goblin.png",  make_blue_goblin()),
    ("red-goblin.png",   make_red_goblin()),
    ("goblin-hit.png",   make_hit_effect()),
]

for fname, img in sprites:
    out_path = os.path.join(OUT, fname)
    img.save(out_path, "PNG")
    write_meta(out_path)
    print(f"  OK  {fname}  ({img.width}x{img.height})")

print("Done.")
