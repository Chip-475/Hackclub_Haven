"""Offline checks on serialized scene geometry; no Unity runtime required."""
from pathlib import Path
import re

scene = Path(__file__).resolve().parents[1] / 'Haven/Assets/Scenes/SampleScene.unity'
text = scene.read_text()
blocks = re.split(r'(?=--- !u!)', text)[1:]
ids = [re.search(r'&(-?\d+)', b)[1] for b in blocks]
assert len(ids) == len(set(ids)), 'Duplicate scene IDs'
assert all(i == '0' or i in ids for i in re.findall(r'\{fileID: (-?\d+)\}', text)), 'Broken scene reference'
objects, transforms = {}, {}
def vector(block, key):
    match = re.search(r'\b' + key + r': \{x: ([^,]+), y: ([^,}]+)', block)
    return tuple(float(v) for v in match.groups())
for block in blocks:
    if block.startswith('--- !u!1 &'):
        objects[re.search(r'&(\d+)', block)[1]] = block
    elif block.startswith('--- !u!4 &'):
        transforms[re.search(r'm_GameObject: \{fileID: (\d+)\}', block)[1]] = block
colliders = []
for block in blocks:
    if not block.startswith('--- !u!61 &') or 'm_Enabled: 0' in block or 'm_IsTrigger: 1' in block:
        continue
    owner = re.search(r'm_GameObject: \{fileID: (\d+)\}', block)[1]
    obj = objects[owner]
    if 'm_Layer: 6\n' not in obj:
        continue
    transform = transforms[owner]
    assert 'm_Father: {fileID: 0}' in transform, 'Ground validation expects root transforms'
    x,y = vector(transform,'m_LocalPosition')
    sx,sy = vector(transform,'m_LocalScale')
    w,h = vector(block,'m_Size')
    ox,oy = vector(block,'m_Offset')
    x += ox*sx; y += oy*sy
    colliders.append((x-w*sx/2,x+w*sx/2,y-h*sy/2,y+h*sy/2))
# Real player body: width .8514, height 2.3787. Sample the entire route.
for i in range(481):
    x = i*.1
    assert any(left <= x <= right and abs(top+3.5)<.02 for left,right,bottom,top in colliders), f'Floor gap at {x}'
    for left,right,bottom,top in colliders:
        overlap = left < x+.426 and right > x-.426 and bottom < -3.48+2.379 and top > -3.48
        assert not overlap, f'Blocked route at {x}: {(left,right,bottom,top)}'
# Check checkpoints have full body clearance too.
for x in [0,12,33]:
    assert not any(l<x+.426 and r>x-.426 and b<-.9 and t>-3.48 for l,r,b,t in colliders), f'Checkpoint blocked at {x}'
for name in ['Spawn','Walk','Attack']:
    assert (scene.parents[1]/f'Resources/Boss/{name}.png').exists(), f'Missing boss {name}'
assert text.count('Assembly-CSharp::GameplayDirector') == 1
print('PASS: unique IDs, local references, 481 floor/clearance samples, 3 checkpoints, 3 boss sheets, director binding.')
