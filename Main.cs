using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.Json;

namespace SendMailTester4 {
    public partial class Main : Form {
        //------------------------------------------------------
        /// <summary>SmtpClientオブジェクト</summary>
        const string setting_filename = ".\\Setting.json";

        //------------------------------------------------------
        /// <summary>SmtpClientオブジェクト</summary>
        System.Net.Mail.SmtpClient m_sc = null;

        //------------------------------------------------------
        /// <summary>コンストラクタ</summary>
        public Main() {
            InitializeComponent();

            //SmtpClientの作成とイベントハンドラの追加
            m_sc = new System.Net.Mail.SmtpClient();
            m_sc.SendCompleted += new System.Net.Mail.SendCompletedEventHandler(this.SendCompleted);

            try {
                //設定ファイルが無ければ終了
                if (!File.Exists(setting_filename)) {
                    return;
                }

                var l_txt = File.ReadAllText(setting_filename);
                Setting l_setting = JsonSerializer.Deserialize<Setting>(l_txt);
                if (l_setting is null) {
                    return;
                }

                txtFrom.Text = l_setting.From;
                txtTo.Text = l_setting.To;
                txtSubject.Text = l_setting.Subject;
                txtBody.Text = l_setting.Body;
                txtHost.Text = l_setting.Host;
                numPort.Value = l_setting.Port;
            } catch (Exception _ex) {
                MessageBox.Show(_ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //------------------------------------------------------
        /// <summary>フォームを閉じる際の処理</summary>
        private void Main_FormClosed(object sender, FormClosedEventArgs e) {
            // SmtpClientの破棄
            m_sc.Dispose();
            m_sc = null;

            //設定値の保存
            try {
                var l_setting = new Setting();
                l_setting.From = txtFrom.Text;
                l_setting.To = txtTo.Text;
                l_setting.Subject = txtSubject.Text;
                l_setting.Body = txtBody.Text;
                l_setting.Host = txtHost.Text;
                l_setting.Port = (int)numPort.Value;
                string l_txt = JsonSerializer.Serialize(l_setting);
                File.WriteAllText(setting_filename, l_txt);
            } catch (Exception _ex) {
                MessageBox.Show(_ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //------------------------------------------------------
        /// <summary>送信ボタン押下時の処理</summary>
        /// <param name="sender">イベント発生オブジェクト</param>
        /// <param name="e">イベントパラメーター</param>
        private void btnSend_Click(object sender, EventArgs e) {
            btnSend.Enabled = false;
            btnCancel.Enabled = true;

            //MailMessageの作成
            System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage(txtFrom.Text,
                                                                              txtTo.Text,
                                                                              txtSubject.Text,
                                                                              txtBody.Text);

            //SMTPサーバーなどを設定する
            m_sc.Host = txtHost.Text;
            m_sc.Port = (int)numPort.Value;
            m_sc.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;

            //メール送信
            //MailMessageをSendCompletedイベントハンドラで取得できるようにする
            m_sc.SendAsync(msg, msg);
        }

        //------------------------------------------------------
        /// <summary>キャンセルボタン押下時の処理</summary>
        /// <param name="sender">イベント発生オブジェクト</param>
        /// <param name="e">イベントパラメーター</param>
        private void btnCancel_Click(object sender, EventArgs e) {
            //メールの送信をキャンセルする
            if (m_sc != null) {
                m_sc.SendAsyncCancel();
            }
        }

        //------------------------------------------------------
        /// <summary>メール送信完了時の処理</summary>
        /// <param name="sender">イベント発生オブジェクト</param>
        /// <param name="e">イベントパラメーター</param>
        private void SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e) {
            //SendAsyncで指定されたMailMessageを取得する
            using (var msg = (System.Net.Mail.MailMessage)e.UserState) {
                if (e.Cancelled) {
                    txtLog.Text += $"{msg.Subject}の送信はキャンセルされました。\r\n";
                } else if (e.Error != null) {
                    txtLog.Text += $"{msg.Subject}の送信でエラーが発生しました。\r\n";
                    txtLog.Text += e.Error.Message + "\r\n";
                } else {
                    txtLog.Text = $"{msg.Subject}の送信が完了しました。\r\n";
                }
            }

            btnSend.Enabled = true;
            btnCancel.Enabled = false;
        }
    }
}
