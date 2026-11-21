using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

// One form per table, each with Add / Update / Delete / Refresh
static class Pages
{
    public static CrudForm Students()
    {
        return new CrudForm("StudentsForm", "tblStudents", "StudentID",
            "INSERT INTO tblStudents(StudentName, Age, YearOfEnroll, Major, DateOfBirth, GPA, City) " +
            "VALUES ('New Student', 20, 2021, 'Major', GETDATE(), 4.0, 'City')",
            "UPDATE tblStudents SET StudentName='Updated Student', Age=21, YearOfEnroll=2022, " +
            "Major='Updated Major', GPA=3.5, City='Updated City' WHERE StudentID=@id");
    }

    public static CrudForm Courses()
    {
        return new CrudForm("CoursesForm", "tblCourses", "CourseID",
            "INSERT INTO tblCourses(CourseName, Credits, DepartmentID, InstructorID) VALUES ('New Course', 3, 1, 1)",
            "UPDATE tblCourses SET CourseName='Updated Course', Credits=4 WHERE CourseID=@id");
    }

    public static CrudForm Instructors()
    {
        return new CrudForm("InstructorsForm", "tblInstructors", "InstructorID",
            "INSERT INTO tblInstructors(InstructorName, DepartmentID) VALUES ('New Instructor', 1)",
            "UPDATE tblInstructors SET InstructorName='Updated Instructor' WHERE InstructorID=@id");
    }

    public static CrudForm Departments()
    {
        return new CrudForm("DepartmentsForm", "tblDepartments", "DepartmentID",
            "INSERT INTO tblDepartments(DepartmentName) VALUES ('New Department')",
            "UPDATE tblDepartments SET DepartmentName='Updated Department' WHERE DepartmentID=@id");
    }
}

class CrudForm : Form
{
    DataGridView grid = new DataGridView
    {
        Location = new Point(20, 20),
        Size = new Size(740, 340),
        ReadOnly = true,
        AllowUserToAddRows = false,
        MultiSelect = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect
    };
    string table, key, insertSql, updateSql;

    public CrudForm(string title, string table, string key, string insertSql, string updateSql)
    {
        this.table = table;
        this.key = key;
        this.insertSql = insertSql;
        this.updateSql = updateSql;

        Text = title + " - Developed by NGUYEN GIA HUY";
        ClientSize = new Size(780, 420);
        StartPosition = FormStartPosition.CenterParent;

        var btnAdd = new Button { Text = "Add", Location = new Point(20, 370), Size = new Size(160, 35) };
        var btnUpdate = new Button { Text = "Update", Location = new Point(215, 370), Size = new Size(160, 35) };
        var btnDelete = new Button { Text = "Delete", Location = new Point(410, 370), Size = new Size(160, 35) };
        var btnRefresh = new Button { Text = "Refresh", Location = new Point(605, 370), Size = new Size(160, 35) };

        btnAdd.Click += (s, e) => Db.Try(() => { Db.Run(insertSql); LoadData(); });
        btnUpdate.Click += (s, e) => Db.Try(() => RunOnSelected(updateSql));
        btnDelete.Click += (s, e) => Db.Try(() => RunOnSelected("DELETE FROM " + table + " WHERE " + key + "=@id"));
        btnRefresh.Click += (s, e) => Db.Try(LoadData);
        Load += (s, e) => Db.Try(LoadData);

        Controls.AddRange(new Control[] { grid, btnAdd, btnUpdate, btnDelete, btnRefresh });
    }

    void LoadData()
    {
        grid.DataSource = Db.Table("SELECT * FROM " + table);
    }

    void RunOnSelected(string sql)
    {
        if (grid.SelectedRows.Count == 0)
        {
            MessageBox.Show("Please select a row first.");
            return;
        }
        object id = grid.SelectedRows[0].Cells[key].Value;
        Db.Run(sql, new SqlParameter("@id", id));
        LoadData();
    }
}

// Pick a student (ComboBox), see only their enrollments, enroll in a course (ComboBox)
class EnrollmentsForm : Form
{
    ComboBox cbStudent = new ComboBox
    {
        Location = new Point(100, 20), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList,
        DisplayMember = "StudentName", ValueMember = "StudentID"
    };
    ComboBox cbCourse = new ComboBox
    {
        Location = new Point(100, 60), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList,
        DisplayMember = "CourseName", ValueMember = "CourseID"
    };
    DataGridView grid = new DataGridView
    {
        Location = new Point(20, 110),
        Size = new Size(540, 220),
        ReadOnly = true,
        AllowUserToAddRows = false,
        MultiSelect = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect
    };

    public EnrollmentsForm()
    {
        Text = "Student Enrollments - Developed by NGUYEN GIA HUY";
        ClientSize = new Size(580, 350);
        StartPosition = FormStartPosition.CenterParent;

        var btnEnroll = new Button { Text = "Enroll", Location = new Point(350, 55), Size = new Size(100, 30) };
        var btnRemove = new Button { Text = "Remove", Location = new Point(460, 55), Size = new Size(100, 30) };
        btnEnroll.Click += (s, e) => Db.Try(Enroll);
        btnRemove.Click += (s, e) => Db.Try(Remove);

        Load += (s, e) => Db.Try(() =>
        {
            cbStudent.DataSource = Db.Table("SELECT StudentID, StudentName FROM tblStudents");
            cbCourse.DataSource = Db.Table("SELECT CourseID, CourseName FROM tblCourses");
            cbStudent.SelectedIndexChanged += (a, b) => Db.Try(LoadEnrollments);
            LoadEnrollments();
        });

        Controls.AddRange(new Control[]
        {
            new Label { Text = "Student", AutoSize = true, Location = new Point(20, 23) },
            new Label { Text = "Course", AutoSize = true, Location = new Point(20, 63) },
            cbStudent, cbCourse, btnEnroll, btnRemove, grid
        });
    }

    void LoadEnrollments()
    {
        if (cbStudent.SelectedValue == null) return;
        grid.DataSource = Db.Table(
            "SELECT e.EnrollmentID, c.CourseName, c.Credits FROM tblEnrollments e " +
            "JOIN tblCourses c ON c.CourseID = e.CourseID WHERE e.StudentID = @s",
            new SqlParameter("@s", cbStudent.SelectedValue));
    }

    void Enroll()
    {
        Db.Run("INSERT INTO tblEnrollments(StudentID, CourseID) VALUES (@s, @c)",
            new SqlParameter("@s", cbStudent.SelectedValue),
            new SqlParameter("@c", cbCourse.SelectedValue));
        LoadEnrollments();
    }

    void Remove()
    {
        if (grid.SelectedRows.Count == 0) return;
        Db.Run("DELETE FROM tblEnrollments WHERE EnrollmentID = @id",
            new SqlParameter("@id", grid.SelectedRows[0].Cells["EnrollmentID"].Value));
        LoadEnrollments();
    }
}
