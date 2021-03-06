import * as Yup from 'yup';

const phoneRegex = /^((0[3|4|5|7|8|9])+([0-9]{8})|(84[3|4|5|7|8|9])+([0-9]{8}))$/;

const websiteRegex = /^(http:\/\/|https:\/\/)?(www.)?([a-zA-Z0-9]+).[a-zA-Z0-9]*.[‌​a-z]{3}\.([a-z]+)?$/

const emailRegex = /^[a-zA-Z0-9\.]+@([\w]+\.)+[\w]{2,4}$/;

const AddContactSchema = Yup.object().shape({
    name: Yup.string()
        .max(255, 'Too Long!')
        .required('Required').nullable(),
    company: Yup.string()
        .max(255, 'Too Long!')
        .required('Required').nullable(),
    phone: Yup.string()
        .matches(phoneRegex, 'Invalid phone number!')
        .required('Required').nullable(),
    email: Yup.string()
        .matches(emailRegex,'Invalid email address!')
        .required('Required').nullable(),
    fax: Yup.string()
        .matches(phoneRegex, 'Invalid fax number!')
        .nullable(),
    // website: Yup.string()
    //     .max(255, 'Too Long!')
    //     .url('Invalid website address!')
    //     .nullable(),
});

export default AddContactSchema;