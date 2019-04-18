﻿using MathLib;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MathLogCenter
{
    public partial class DeleteStudent : UserControl
    {

        private bool validated = false;
        private bool validId = false;
        private bool validFName = false;
        private bool validLName = false;
        private bool validClassNum = false;
        public delegate void UpdateStudentEvent();
        public event UpdateStudentEvent UpdateStudentEventHandler;
        private StudentRecord student;
        // I can share maybe via student record???
        public DeleteStudent()
        {
            InitializeComponent();
            this.radioMath.Checked = true;
            this.radioID.Checked = true;
            cmbClassNum.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbClassNum.Items.AddRange(DConnect.Connection.GetClassList("MTH"));

            if (cmbClassNum.Items.Count < 1)
            {
                cmbClassNum.Text = string.Empty;
            }
            else
            {
                cmbClassNum.SelectedIndex = 0;
            }


            SetupRadioButtonEvents();
            btnDelete.Enabled = false;
        }

        public DeleteStudent(string id, string first, string last, string classType, string classNum)
        {
            InitializeComponent();

            this.txtID.Text = id.ToUpper();
            this.txtFirst.Text = first.ToUpper();
            this.txtLast.Text = last.ToUpper();
            this.cmbClassNum.Text = classNum.ToUpper();
            if (classType.Contains("MTH"))
            {
                this.radioMath.Checked = true;
                this.radioOther.Checked = false;
                this.radioStat.Checked = false;
                cmbClassNum.Items.AddRange(DConnect.Connection.GetClassList("MTH"));
            }
            else if (classType.Contains("STAT"))
            {
                this.radioMath.Checked = false;
                this.radioOther.Checked = false;
                this.radioStat.Checked = true;
                cmbClassNum.Items.AddRange(DConnect.Connection.GetClassList("STAT"));
            }
            else
            {
                this.radioMath.Checked = false;
                this.radioOther.Checked = true;
                this.radioStat.Checked = false;
                cmbClassNum.Items.AddRange(DConnect.Connection.GetClassList("OTH"));
            }

            if (cmbClassNum.Items.Count < 1)
            {
                cmbClassNum.Text = string.Empty;
            }
            else
            {
                cmbClassNum.SelectedIndex = 0;
            }
            this.radioID.Checked = true;
            this.radioName.Checked = false;
            this.chkFilters.Checked = true;
            cmbClassNum.DropDownStyle = ComboBoxStyle.DropDownList;
            SetupRadioButtonEvents();
            btnDelete.Enabled = false;
        }



        #region Buttons
        private void btnValidate_Click(object sender, EventArgs e)
        {
            normal_colors();
            validator();

            if (validated)
            {
                normal_colors();
            }
            else
            {
                color_errors();
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {


                string searchString = "";
                // Validate of course
                validator();
                // Do some student record stuff here
                if (validated)
                {
                    normal_colors();

                    if (radioID.Checked && DConnect.Connection.StudentExists(txtID.Text.Trim()))
                    {

                        // Apply the filters...
                        if (chkFilters.Checked)
                        {
                            // TODO Search and get results for all records by ID

                            searchString += "StudentID = " + txtID.Text.Trim();
                            searchString += " AND ClassType = '" + Utility.WordFromBool(radioMath.Checked, radioStat.Checked, radioOther.Checked) + "'";
                            if (!string.IsNullOrEmpty(cmbClassNum.Text))
                            {
                                searchString += " AND ClassNum = '" + cmbClassNum.Text + "'";
                            }

                            bool isSuccess;
                            student = DConnect.Connection.GetRecordBySearchString(searchString, out isSuccess);
                            if (isSuccess)
                            {
                                // Display the student here
                                display_student(student);
                            }
                            else
                            {
                                if (DConnect.Connection.GetSetting("ShowFormPopUp"))
                                {
                                    MessageBox.Show("Student Does Not Exist", "Invalid Criteria", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                           

                        }
                        else
                        {
                            student = DConnect.Connection.GetRecordByID(this.txtID.Text);
                            // Display the student here
                            display_student(student);
                        }
                    }
                    // Name is checked..search by first and last name
                    else if (radioName.Checked && DConnect.Connection.StudentExists(DConnect.Connection.GetRecordByName(txtFirst.Text, txtLast.Text).studentID))
                    {
                        // Apply the filters...
                        if (chkFilters.Checked)
                        {
                            searchString += "FirstName = '" + txtFirst.Text.ToUpper().Trim() + "'";
                            searchString += " AND LastName = '" + txtLast.Text.ToUpper().Trim() + "'";
                            // TODO Search and get results for all records by NAME
                            searchString += " AND ClassType = '" + Utility.WordFromBool(radioMath.Checked, radioStat.Checked, radioOther.Checked) + "'";
                            if (!string.IsNullOrEmpty(cmbClassNum.Text))
                            {
                                searchString += " AND ClassNum = '" + cmbClassNum.Text + "'";

                            }

                            bool isSuccess;
                            student = DConnect.Connection.GetRecordBySearchString(searchString, out isSuccess);
                            if (isSuccess)
                            {
                                // Display the student here
                                display_student(student);
                            }
                            else
                            {
                                if (DConnect.Connection.GetSetting("ShowFormPopUp"))
                                {
                                    MessageBox.Show("Student Does Not Exist", "Invalid Criteria", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                        else
                        {
                            student = DConnect.Connection.GetRecordByName(this.txtFirst.Text, this.txtLast.Text);
                            // Display the student here
                            display_student(student);
                        }
                    }
                     else
                    {
                        if (DConnect.Connection.GetSetting("ShowFormPopUp"))
                        {
                            MessageBox.Show("Student Does Not Exist", "Invalid Criteria", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }


                }
                // If not validated
                else
                {
                    color_errors();
                }
            }
            catch (Exception ex)
            {
                MathLib.Logger.Instance.WriteLog("ERROR", ex.Message + "in SearchStudent.Search");
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            reset();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            // If not checked in...
            // Validate of course
            validator();
            // Do some student record stuff here
            if (validated)
            {
                normal_colors();
                if (!DConnect.Connection.IsCheckedIn(student.studentID))
                {
                    if (DConnect.Connection.DeleteStudentByID(student.studentID))
                    {
                        if (DConnect.Connection.GetSetting("ShowFormPopUp"))
                            MessageBox.Show("Successfully Deleted!", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        if (UpdateStudentEventHandler != null)
                        {
                            UpdateStudentEventHandler();
                        }

                        reset();
                    }
                    else
                    {
                        if (DConnect.Connection.GetSetting("ShowFormPopUp"))
                            MessageBox.Show("Could Not Delete!", "Failure!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    if (DConnect.Connection.GetSetting("ShowFormPopUp"))
                        MessageBox.Show("Student Is still logged in...", "Failure!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                  
            }
            
        }

        #endregion

        #region Helper Methods
      
        private void display_student(StudentRecord stu)
        {
            this.lblShowID.Text = stu.studentID;
            this.lblShowFirst.Text = stu.firstName;
            this.lblShowLast.Text = stu.lastName;
            this.lblShowSemester.Text = stu.semester;
            this.lblShowType.Text = stu.classType;
            this.lblShowClassNum.Text = stu.classNum;
            this.lblShowTime.Text = stu.totalTime;
            this.lblShowVisits.Text = stu.numVisits;
            btnDelete.Enabled = true;
        }
        private void SetupRadioButtonEvents()
        {
            radioMath.CheckedChanged += (object obj, EventArgs e) =>
            {
                this.cmbClassNum.Items.Clear();

                if (radioMath.Checked)
                    cmbClassNum.Items.AddRange(DConnect.Connection.GetClassList("MTH"));

                if (cmbClassNum.Items.Count < 1)
                {
                    cmbClassNum.Text = string.Empty;
                }
                else
                {
                    cmbClassNum.SelectedIndex = 0;
                }
            };

            radioStat.CheckedChanged += (object obj, EventArgs e) =>
            {

                this.cmbClassNum.Items.Clear();
                if (radioStat.Checked)
                    cmbClassNum.Items.AddRange(DConnect.Connection.GetClassList("STAT"));

                if (cmbClassNum.Items.Count < 1)
                {
                    cmbClassNum.Text = string.Empty;
                }
                else
                {
                    cmbClassNum.SelectedIndex = 0;
                }
            };

            radioOther.CheckedChanged += (object obj, EventArgs e) =>
            {
                this.cmbClassNum.Items.Clear();
                if (radioOther.Checked)
                    cmbClassNum.Items.AddRange(DConnect.Connection.GetClassList("OTH"));

                if (cmbClassNum.Items.Count < 1)
                {
                    cmbClassNum.Text = string.Empty;
                }
                else
                {
                    cmbClassNum.SelectedIndex = 0;
                }

            };

        }
        private void reset()
        {
            this.txtFirst.Text = string.Empty;
            this.txtID.Text = string.Empty;
            this.txtLast.Text = string.Empty;
            this.cmbClassNum.Text = string.Empty;
            radioID.Checked = true;
            radioMath.Checked = true;
            chkFilters.Checked = false;
            radioName.Checked = false;
            radioStat.Checked = false;
            radioOther.Checked = false;

            this.lblShowClassNum.Text = "###";
            this.lblShowType.Text = "###";
            this.lblShowFirst.Text = "###";
            this.lblShowID.Text = "###";
            this.lblShowLast.Text = "###";
            this.lblShowSemester.Text = "###";
            this.lblShowTime.Text = "###";
            this.lblShowVisits.Text = "###";
            this.btnDelete.Enabled = false;
            normal_colors();
        }



        // Does a bunch of validation work to take the errors out of the hands of the user..
        private void validator()
        {
            string first_name = this.txtFirst.Text.Trim();
            string last_name = this.txtLast.Text.Trim();
            string id = this.txtID.Text.Trim();
            string class_num = this.cmbClassNum.Text.Trim();

            // If only the ID is checked
            if (radioID.Checked)
            {
                // If filters are to be applied
                if (this.chkFilters.Checked)
                {

                    // If there is a class num and class type
                    if ((radioMath.Checked || radioStat.Checked || radioOther.Checked) && !string.IsNullOrEmpty(class_num))
                    {
                        validClassNum = MathLib.Validate.number(class_num) && class_num.Length < 5;
                        validId = MathLib.Validate.number(id) && id.Length > 5;

                        if (validClassNum && validId)
                        {
                            validated = true;
                        }
                        else
                        {
                            validated = false;
                        }
                    }
                    // If only class type filter is applied
                    else
                    {
                        validId = MathLib.Validate.number(id) && id.Length > 5;
                        if (validId)
                        {
                            validated = true;
                        }
                        else
                        {
                            validated = false;
                        }
                    }
                }
                // If the filter is not checked..
                else
                {
                    validId = MathLib.Validate.number(id) && id.Length > 5;
                    if (validId)
                    {
                        validated = true;
                    }
                    else
                    {
                        validated = false;
                    }
                }

            }
            // If the name radio button is checked
            else
            {
                // If the filters are applied
                if (this.chkFilters.Checked)
                {
                    // If there is a classtype and class num
                    if ((radioMath.Checked || radioStat.Checked || radioOther.Checked) && !string.IsNullOrEmpty(class_num))
                    {
                        validClassNum = MathLib.Validate.number(class_num) && class_num.Length < 5;
                        validFName = MathLib.Validate.loose_string(first_name) && first_name.Length < 40 && !string.IsNullOrEmpty(first_name);
                        validLName = MathLib.Validate.loose_string(last_name) && last_name.Length < 40 && !string.IsNullOrEmpty(last_name);
                        if (validFName && validLName && validClassNum)
                        {
                            validated = true;
                        }
                        else
                        {
                            validated = false;
                        }
                    }
                    // If there is not a class num but a classtype
                    else
                    {
                        validFName = MathLib.Validate.loose_string(first_name) && first_name.Length < 40 && !string.IsNullOrEmpty(first_name);
                        validLName = MathLib.Validate.loose_string(last_name) && last_name.Length < 40 && !string.IsNullOrEmpty(last_name);
                        if (validFName && validLName)
                        {
                            validated = true;
                        }
                        else
                        {
                            validated = false;
                        }
                    }
                }
                // If no filters are applied
                else
                {
                    validFName = MathLib.Validate.loose_string(first_name) && first_name.Length < 40 && !string.IsNullOrEmpty(first_name);
                    validLName = MathLib.Validate.loose_string(last_name) && last_name.Length < 40 && !string.IsNullOrEmpty(last_name);
                    if (validFName && validLName)
                    {
                        validated = true;
                    }
                    else
                    {
                        validated = false;
                    }
                }

            }

        }

        private void color_errors()
        {
            if (radioID.Checked)
            {
                if (!validId)
                {
                    this.txtID.BackColor = Color.LightGreen;
                }

                if (chkFilters.Checked)
                {
                    if (!validClassNum)
                    {
                        this.cmbClassNum.BackColor = Color.LightGreen;
                    }
                }
            }
            else
            {

                if (!validFName)
                {
                    this.txtFirst.BackColor = Color.LightGreen;
                }
                if (!validLName)
                {
                    this.txtLast.BackColor = Color.LightGreen;
                }


                if (chkFilters.Checked)
                {
                    if (!validClassNum)
                    {
                        this.cmbClassNum.BackColor = Color.LightGreen;
                    }
                }
            }
        }
        private void normal_colors()
        {
            this.txtFirst.BackColor = Color.White;
            this.txtLast.BackColor = Color.White;
            this.txtID.BackColor = Color.White;
            this.cmbClassNum.BackColor = Color.White;
        }

        #endregion

       
    }
}