﻿using RatChat.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RatChat {
    public class VisualMessage {
        const string LinkReplacer = "%LINKLINK%";
        static Regex UriDetector = new Regex(@"((http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)");

        public FrameworkElement Text { get; private set; }

        public ChatMessage Data { get; private set; }
        public bool NeedDelete { get; set; }
        public string TalkTo { get; set; }

        public VisualMessage( string StreamerNick, SmilesDataDase Db, ChatMessage Data ) {
            this.Data = Data;

            List<Uri> Urls = new List<Uri>();

            string UserText = HttpUtility.HtmlDecode(Data.Text.Replace(":s:", " :s:").Replace("  ", " "));

            UserText = UriDetector.Replace(
                UserText,
                new MatchEvaluator(( m ) => {
                    Urls.Add(new Uri(m.Value, UriKind.RelativeOrAbsolute));
                    return LinkReplacer + " ";
                })
            );

            // parse text
            WrapPanel wp = new WrapPanel() {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
            };
            List<string> ttt = new List<string>();

            // Тоже странный кусок:
            int nxd = UserText.IndexOf("<b>");
            if (nxd >= 0) {
                int nxd2 = UserText.IndexOf("</b>");
                TalkTo = UserText.Substring(nxd + 3, nxd2 - nxd - 3);
                UserText = UserText.Substring(nxd2 + 6);

                if (TalkTo == StreamerNick) {
                    wp.SetResourceReference(WrapPanel.StyleProperty, "StreamerContainer");
                } else {
                    wp.SetResourceReference(WrapPanel.StyleProperty, "NormalTextContainer");
                }

                ttt.Add(TalkTo + ",");
            } else {
                TalkTo = "";
            }

            ttt.AddRange(UserText.Split(' '));

            TextBlock name = new TextBlock() { Text = Data.Name + ": " };
            name.SetResourceReference(TextBlock.StyleProperty, "NameStyle");
            wp.Children.Add(name);
            int linkIndex = 0;

            for (int j = 0; j < ttt.Count; ++j) {
                if (ttt[j] == LinkReplacer) {
                    TextBlock link = new TextBlock() {
                        Text = "link ",
                        Cursor = Cursors.Hand,
                        ToolTip = Urls[linkIndex],
                        Tag = Urls[linkIndex]
                    };
                    link.SetResourceReference(TextBlock.StyleProperty, "LinkStyle");
                    link.MouseLeftButtonUp += ( sender, b ) => {
                        Uri u = ((TextBlock)sender).Tag as Uri;
                        System.Diagnostics.Process.Start(u.ToString());
                    };
                    wp.Children.Add(link);
                    linkIndex++;
                } else {
                    //if (j != (ttt.Count - 1))
                        ttt[j] += ' ';

                    if (CreateSmile(Db, ttt[j], wp)) {
                        // Ура смайл ебать есть
                    } else {
                        TextBlock txt = new TextBlock() { Text = ttt[j] };
                        if (TalkTo + ", " == ttt[j]) {
                            txt.SetResourceReference(TextBlock.StyleProperty, "NameTextStyle");
                        } else {
                            txt.SetResourceReference(TextBlock.StyleProperty, "TextStyle");
                        }
                        wp.Children.Add(txt);
                    }
                }
            }

            Text = wp;
        }

        private bool CreateSmile( SmilesDataDase Db, string SmileId, WrapPanel TextPanel ) {

            if (Properties.Settings.Default.hideSmiles) {
                return false;
            } else {
                //int nn = SmileId.LastIndexOf(':');
                //SmileId = SmileId.Substring(2, nn + 1 - 2) + " ";

                
                FrameworkElement s = Db.GetSmile(SmileId);
                if (s != null) {
                    TextPanel.Children.Add(s);
                    return true;
                }
            }

            return false;
        }
    }
}
