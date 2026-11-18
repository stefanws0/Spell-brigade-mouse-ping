"""Turn the badge art (art/icon_source.png) into the Thunderstore icon: 256x256 PNG, transparent outside the badge.

Usage: python -I tools/make_icon.py art/icon_source.png thunderstore/icon.png

The source is a round badge in the game's icon style (white ring, black disc, rough white silhouette) on an opaque
background. The script finds the white ring's outer edge along the middle row and column, keeps everything inside
that circle (anti-aliased edge) and makes the rest transparent.
"""
import math
import sys

from PIL import Image

SIZE = 256
BRIGHT = 200  # a pixel this bright (average of R, G, B) belongs to the white ring


def outer_edge(values):
    """Index of the first and last bright value: the ring's outer edge along one line."""
    bright = [i for i, v in enumerate(values) if v >= BRIGHT]
    if not bright:
        raise SystemExit("no white ring found along the middle of the image")
    return bright[0], bright[-1]


def main(src, dst):
    img = Image.open(src).convert("RGB")
    w, h = img.size
    gray = img.convert("L")
    px = gray.load()
    left, right = outer_edge([px[x, h // 2] for x in range(w)])
    top, bottom = outer_edge([px[w // 2, y] for y in range(h)])
    cx, cy = (left + right + 1) / 2, (top + bottom + 1) / 2
    r = (right - left + bottom - top + 2) / 4
    print(f"{src} {img.size}: ring centre ({cx:.0f}, {cy:.0f}), radius {r:.0f}")

    alpha = Image.new("L", (w, h), 0)
    a = alpha.load()
    for y in range(h):
        for x in range(w):
            d = math.hypot(x + 0.5 - cx, y + 0.5 - cy)
            a[x, y] = int(max(0.0, min(1.0, r - d + 0.5)) * 255)
    badge = img.convert("RGBA")
    badge.putalpha(alpha)
    half = r + 2
    badge = badge.crop((round(cx - half), round(cy - half), round(cx + half), round(cy + half)))
    badge.resize((SIZE, SIZE), Image.LANCZOS).save(dst)
    print(f"wrote {dst} ({SIZE}x{SIZE})")


if __name__ == "__main__":
    if len(sys.argv) != 3:
        raise SystemExit(__doc__)
    main(sys.argv[1], sys.argv[2])
