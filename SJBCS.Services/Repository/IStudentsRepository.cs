﻿using SJBCS.Data;
using System;
using System.Collections.Generic;

namespace SJBCS.Services.Repository
{
    public interface IStudentsRepository
    {
        List<Student> GetStudents();
        Student GetStudent(Guid id);
        Student GetStudent(string id);
        Student AddStudent(Student Student);
        Student UpdateStudent(Student Student);
        void DeleteStudent(Guid id);
    }
}
