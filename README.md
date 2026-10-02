# PoseBridge — הקמה מחדש במק

החבילה כוללת את הגרסה האחרונה שעבדה בשיחה:
אתר באנגלית, בחירת מצלמה אחת מתוך המצלמות הזמינות, MediaPipe ב־Worker רגיל,
גישור WebSocket → UDP, הפעלה בלחיצה כפולה ב־Safari ושלושה סקריפטים ליוניטי 2D.
החבילה אינה מכילה את סצנת Unity שנמחקה.

## 1. התקנת הגישור — פעם אחת

חלצי את החבילה. השאירי את כל תיקיית PoseBridge יחד.
צריך Python 3.10 ומעלה. לבדיקה: python3 --version.
ב־Terminal הקלידי cd ואחריו רווח, גררי את תיקיית PoseBridge ולחצי Enter.
הריצי כל פקודה בנפרד:

```bash
python3 -m venv .venv
```

```bash
.venv/bin/python -m pip install -r requirements.txt
```

```bash
chmod +x Start-PoseBridge.command
```

## 2. הפעלה בכל פעם

לחצי פעמיים על Start-PoseBridge.command בתוך PoseBridge.
הוא מפעיל את bridge.py, מחכה לעמוד ופותח Safari.
השאירי את הטרמינל פתוח. העמוד: http://127.0.0.1:8000

לחצי Refresh cameras, אשרי מצלמה, בחרי Camera ולחצי Start camera.
אפשר לעבור בין מצלמות גם בזמן זיהוי. בכל רגע רק אחת שולחת נתונים ליוניטי.
לאייפון: Continuity Camera מופעל, המכשיר זמין למק, FaceTime סגור.
המצלמה צריכה לראות את הגוף; למבט צד אפשר למקם אותה בצד השחקן.
הדף והמודל דורשים חיבור לאינטרנט בעת הטעינה.

אם הפורט תפוס, עצרי את הגישור הקודם ב־Control+C ונסי שוב.
לסיום: עצרי Play ביוניטי, לחצי Stop בדף ו־Control+C בטרמינל.

## 3. פרויקט Unity

צרי פרויקט 2D חדש. גררי את שלושת הקבצים מתוך Unity לתיקיית Assets:

| סקריפט | תפקיד |
|---|---|
| PoseReceiver.cs | קליטת כל 33 המפרקים; כולל enum בשם Joint |
| MoveWithJoint.cs | תנועה של עיגול לפי מפרק |
| StickFigureManager.cs | יצירת קווים וחישוב צוואר באמצע הכתפיים |

צרי Empty GameObject בשם BodyInput והוסיפי לו PoseReceiver.
השאירי Port = 5052, Mirror X מסומן ו־Minimum Visibility = 0.5.
התיקון כבר כלול: הסינון משתמש ב־visibility ואינו חוסם נקודות בגלל presence חסר.

## 4. עיגולי המפרקים

צרי Circle דרך GameObject → 2D Object → Sprites → Circle.
הקטיני אותו, למשל Scale = (0.15, 0.15, 1), והוסיפי MoveWithJoint.
גררי את BodyInput לשדה Body. שכפלי ל־13 עיגולים והגדירי:

| שם האובייקט | Joint ב־MoveWithJoint | השדה במנהל |
|---|---|---|
| jointNose | Nose | Nose |
| jointLShoulder | LeftShoulder | Left Shoulder |
| jointRShoulder | RightShoulder | Right Shoulder |
| jointLElbow | LeftElbow | Left Elbow |
| jointRElbow | RightElbow | Right Elbow |
| jointLWrist | LeftWrist | Left Wrist |
| jointRWrist | RightWrist | Right Wrist |
| jointLHip | LeftHip | Left Hip |
| jointRHip | RightHip | Right Hip |
| jointLKnee | LeftKnee | Left Knee |
| jointRKnee | RightKnee | Right Knee |
| jointLHeel | LeftHeel | Left Heel |
| jointRHeel | RightHeel | Right Heel |

ב־Inspector יוניטי עשוי להציג שמות עם רווח, למשל Left Shoulder.
Left/Right הם צדדי הגוף של השחקן.
בכל העיגולים הגדירי Z = 0, Center = (0,0), Movement Size = (10,6).
Smoothing = 15 הוא ערך ההתחלה.
שמרי את אותן הגדרות אזור תנועה ומרכז לכל המפרקים.

## 5. הקווים

צרי Empty GameObject בשם StickFigure והוסיפי StickFigureManager.
גררי את 13 העיגולים לשדות המתאימים, פעם אחת לכל מפרק.
צרי Material דרך Create → Material ובחרי Shader: Sprites/Default.
השאירי את צבע החומר לבן וגררי אותו לשדה Line Material.
Line Width = 0.08, Sorting Order = -1, Line Z = 0.
העיגולים ב־Sorting Layer Default ו־Order in Layer = 0.

ב־Play נוצרים 13 קווים ו־Neck ריק, מחושב באמצע בין הכתפיים.
ממנו יוצא קו יחיד לאף. אין צורך ב־JointLine או בחיבור ידני של קווים.
אפשר לשנות צבע ועובי במנהל בזמן Play; שמרי ערכים רצויים גם מחוץ ל־Play.

## 6. בדיקה ושמירה

השאירי את Safari גלוי לצד יוניטי והפעילי Play.
ב־BodyInput: Receiving מציין קבלת נתונים, Tracking מציין זיהוי גוף,
Right Wrist מציג את מיקום היד כשאיכות הזיהוי מספיקה.
אם נקודה לא מזוהה מספיק טוב, העיגול נשאר במיקומו האחרון.
אין בחבילה כיווץ רוחב או הטיה מלאכותית של הדמות.

שמרי את הסצנה עם Command+S מחוץ ל־Play.
X נע מ־0 בשמאל ל־1 בימין; Y נע מ־0 למטה ל־1 למעלה.
זו קליטה ליוניטי Editor או אפליקציית Desktop באותו מחשב, לא Unity WebGL.
