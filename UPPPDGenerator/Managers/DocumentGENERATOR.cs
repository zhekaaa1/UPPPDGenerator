using X = DocumentFormat.OpenXml.Packaging;
using Y = DocumentFormat.OpenXml.Wordprocessing;
using A = DocumentFormat.OpenXml.Drawing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using System;
using System.IO;
using System.Windows;
using UPPPDGenerator.DocumentSettings;
using System.Windows.Controls;
using UPPPDGenerator.Windows;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Settings = UPPPDGenerator.DocumentSettings.Settings;

namespace UPPPDGenerator.Managers
{
    public class DocumentGENERATOR
    {
        public void GenerateDocument(TemplateJsonStructure templateDecoded, StackPanel DocumentPanel, out string outputPathStr)
        {
            string outputPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\Документ_{DateTime.Now:yyyyMMdd_HHmmss}.docx";

            using (WordprocessingDocument doc = WordprocessingDocument.Create(outputPath, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = doc.AddMainDocumentPart();
                mainPart.Document = new Document();

                Body body = new Body();
                if (!string.IsNullOrEmpty(templateDecoded.TitlePageXml))
                {
                    body.InnerXml = templateDecoded.TitlePageXml;
                    doc.MainDocumentPart.Document.Save();
                    doc.Save();
                }

                mainPart.Document.Append(body);
                mainPart.Document.Save();
            }
            using (X.WordprocessingDocument doc = WordprocessingDocument.Open(outputPath, true))
            {
                var body = doc.MainDocumentPart.Document.Body;
                foreach (var element in DocumentPanel.Children)
                {
                    if (element is RichTextBox richTextBox)
                    {
                        InsertText(body, richTextBox, templateDecoded.DocumentSettings.TextFieldSettings);
                    }
                    else if (element is StackPanel imagePanel)
                    {
                        ImageResource imageResource = imagePanel.Tag as ImageResource;
                        if (imageResource != null)
                        {
                            InsertImage(body, imageResource, templateDecoded.DocumentSettings,doc);
                        }
                    }
                }

                var pageSettings = templateDecoded.DocumentSettings.PageSettings;

                int top = (int)(pageSettings.TopMargin * 567);
                int bottom = (int)(pageSettings.BottomMargin * 567);
                int left = (int)(pageSettings.LeftMargin * 567);
                int right = (int)(pageSettings.RightMargin * 567);

                var sectionProps = body.GetFirstChild<SectionProperties>();
                if (sectionProps == null)
                {
                    sectionProps = new SectionProperties();
                    body.Append(sectionProps);
                }

                sectionProps.RemoveAllChildren<PageMargin>();

                sectionProps.AppendChild(new PageMargin
                {
                    Top = new Int32Value(top),
                    Bottom = new Int32Value(bottom),
                    Left = (UInt32Value)(uint)left,
                    Right = (UInt32Value)(uint)right,
                });

                doc.MainDocumentPart.Document.Save();
            }
            outputPathStr = outputPath;
        }
        private void InsertText(Y.Body body, RichTextBox rtb, TextFieldSettings settings)
        {
            foreach (var block in rtb.Document.Blocks)
            {
                if (block is System.Windows.Documents.Paragraph wpfParagraph)
                {
                    var paragraph = new Y.Paragraph();
                    var paragraphProperties = new Y.ParagraphProperties();
                    if (settings.Alignment == "По ширине")
                    {
                        paragraphProperties.Justification = new Y.Justification { Val = Y.JustificationValues.Both };
                    }
                    else if (settings.Alignment == "По правому краю")
                    {
                        paragraphProperties.Justification = new Y.Justification { Val = Y.JustificationValues.Right };
                    }
                    else
                    {
                        paragraphProperties.Justification = new Y.Justification { Val = Y.JustificationValues.Left };
                    }
                    var spacing = new Y.SpacingBetweenLines
                    {
                        Before = (settings.MarginTop * 20).ToString("F0"),
                        After = (settings.MarginBottom * 20).ToString("F0"),
                    };
                    if (settings.LineSpacingType == "Множитель")
                    {
                        spacing.LineRule = Y.LineSpacingRuleValues.Auto;
                        spacing.Line = (settings.LineSpacingMultiplier * 240).ToString("F0");
                    }
                    else if (settings.LineSpacingType == "Одинарный") spacing.Line = "240";
                    else if (settings.LineSpacingType == "Полуторный") spacing.Line = "360";
                    else if (settings.LineSpacingType == "Двойной") spacing.Line = "480";
                    paragraphProperties.SpacingBetweenLines = spacing;
                    if (settings.FirstLineIndentation > 0)
                    {
                        paragraphProperties.Indentation = new Y.Indentation
                        {
                            FirstLine = ((int)(settings.FirstLineIndentation * 567)).ToString()
                        };
                    }
                    paragraph.ParagraphProperties = paragraphProperties;
                    foreach (var inline in wpfParagraph.Inlines)
                    {
                        if (inline is System.Windows.Documents.Run wpfRun)
                        {
                            var run = new Y.Run(new Y.Text(wpfRun.Text) { Space = SpaceProcessingModeValues.Preserve });
                            var runProps = new Y.RunProperties();
                            if (wpfRun.FontWeight == FontWeights.Bold)
                                runProps.Bold = new Y.Bold();
                            if (wpfRun.FontStyle == FontStyles.Italic)
                                runProps.Italic = new Y.Italic();
                            if (wpfRun.TextDecorations.Contains(System.Windows.TextDecorations.Underline[0]))
                                runProps.Underline = new Y.Underline { Val = Y.UnderlineValues.Single };
                            if (!string.IsNullOrEmpty(settings.FontFamily))
                                runProps.RunFonts = new Y.RunFonts { Ascii = settings.FontFamily, HighAnsi = settings.FontFamily };
                            if (settings.FontSize > 0)
                                runProps.FontSize = new Y.FontSize { Val = (settings.FontSize * 2).ToString() };
                            run.RunProperties = runProps;
                            paragraph.Append(run);
                        }
                    }
                    body.Append(paragraph);
                }
            }
        }
        private void InsertImage(Y.Body body, ImageResource imageResource, Settings allSettings,WordprocessingDocument doc)
        {
            if (imageResource == null) return; 
            ImageSettings settings = allSettings.ImageSettings;
            var paragraph = new Y.Paragraph();
            var paragraphProperties = new Y.ParagraphProperties();
            if (settings.Alignment == "По центру")
            {
                paragraphProperties.Justification = new Y.Justification { Val = Y.JustificationValues.Center };
            }
            else if (settings.Alignment == "По правому краю")
            {
                paragraphProperties.Justification = new Y.Justification { Val = Y.JustificationValues.Right };
            }
            else
            {
                paragraphProperties.Justification = new Y.Justification { Val = Y.JustificationValues.Left };
            }
            paragraph.Append(paragraphProperties);
            string imagePath = imageResource.FilePath;
            var imagePart = doc.MainDocumentPart.AddImagePart(X.ImagePartType.Jpeg);
            using (var stream = File.OpenRead(imagePath))
            {
                imagePart.FeedData(stream);
            }
            string relationshipId = doc.MainDocumentPart.GetIdOfPart(imagePart);
            long cx = 5500000;
            long cy = 3200000;
            var element = new Y.Drawing(
        new Inline(
            new Extent { Cx = cx, Cy = cy },
            new DocProperties { Id = (UInt32Value)1U, Name = Path.GetFileName(imageResource.FilePath) },
            new NonVisualGraphicFrameDrawingProperties(new A.GraphicFrameLocks { NoChangeAspect = true }),
            new A.Graphic(
                new A.GraphicData(
                    new PIC.Picture(
                        new PIC.NonVisualPictureProperties(
                            new PIC.NonVisualDrawingProperties { Id = 0U, Name = Path.GetFileName(imageResource.FilePath) },
                            new PIC.NonVisualPictureDrawingProperties()),
            new PIC.BlipFill(
                            new A.Blip { Embed = relationshipId },
                            new A.Stretch(new A.FillRectangle())),
                        new PIC.ShapeProperties(
                            new A.Transform2D(
                                new A.Offset { X = 0, Y = 0 },
                                new A.Extents { Cx = cx, Cy = cy }),
                            new A.PresetGeometry { Preset = A.ShapeTypeValues.Rectangle }))
                )
                { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
        )
        {
            DistanceFromTop = 0U,
            DistanceFromBottom = 0U,
            DistanceFromLeft = 0U,
            DistanceFromRight = 0U
        });
            var run = new Y.Run(element);
            paragraph.Append(run);
            if (settings.EnableNumbering || settings.EnableDescriptions)
            {
                string captionText = string.Empty;
                if (settings.EnableNumbering)
                {
                    captionText = $"Рисунок {imageResource.Id}";
                }
                if (settings.EnableDescriptions)
                {
                    if (!string.IsNullOrEmpty(captionText))
                        captionText += " — ";
                    captionText += imageResource.Description;
                }
                TextFieldSettings tsettings = allSettings.TextFieldSettings;
                var captionParagraph = new Y.Paragraph();
                var tparagraphProperties = new Y.ParagraphProperties();

                if (settings.Alignment == "По ширине")
                {
                    tparagraphProperties.Justification = new Y.Justification { Val = Y.JustificationValues.Both };
                }
                else if (settings.Alignment == "По правому краю")
                {
                    tparagraphProperties.Justification = new Y.Justification { Val = Y.JustificationValues.Right };
                }
                else if (settings.Alignment == "По левому краю")
                {
                    tparagraphProperties.Justification = new Y.Justification { Val = Y.JustificationValues.Left };
                }
                else
                {
                    tparagraphProperties.Justification = new Y.Justification { Val = Y.JustificationValues.Center };
                }

                var spacing = new Y.SpacingBetweenLines
                {
                    Before = (tsettings.MarginTop * 20).ToString("F0"),
                    After = (tsettings.MarginBottom * 20).ToString("F0"),
                };

                if (tsettings.LineSpacingType == "Множитель")
                {
                    spacing.LineRule = Y.LineSpacingRuleValues.Auto;
                    spacing.Line = (tsettings.LineSpacingMultiplier * 240).ToString("F0");
                }
                else if (tsettings.LineSpacingType == "Одинарный") spacing.Line = "240";
                else if (tsettings.LineSpacingType == "Полуторный") spacing.Line = "360";
                else if (tsettings.LineSpacingType == "Двойной") spacing.Line = "480";

                tparagraphProperties.SpacingBetweenLines = spacing;

                captionParagraph.ParagraphProperties = tparagraphProperties;
                var captionRun = new Y.Run(new Y.Text(captionText));
                Y.RunProperties runProps = new Y.RunProperties();

                if (!string.IsNullOrEmpty(tsettings.FontFamily))
                    runProps.RunFonts = new Y.RunFonts { Ascii = tsettings.FontFamily, HighAnsi = tsettings.FontFamily };

                if (tsettings.FontSize > 0)
                    runProps.FontSize = new Y.FontSize { Val = (tsettings.FontSize * 2).ToString() };
                captionRun.RunProperties = runProps;

                captionParagraph.Append(captionRun);

                body.Append(paragraph);  
                body.Append(captionParagraph);
            }
            else
            {
                body.Append(paragraph);
            }
        }
    }
}
