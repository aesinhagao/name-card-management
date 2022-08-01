//import liraries
import React, { useState, useRef, useEffect, useContext } from "react";
import { View, Image, Dimensions, Keyboard} from "react-native";
import { Provider, Button } from "react-native-paper";

import { StackActions } from "@react-navigation/native";
import styles from "./styles";
import { FetchApi } from "../../service/api/FetchAPI";
import { ContactAPI, Method, ContentType } from "../../constants/ListAPI";
import { Formik } from "formik";
import AddContactSchema from "../../validate/ValidateFormAddContact";
import { createShimmerPlaceholder } from "react-native-shimmer-placeholder";
import { LinearGradient } from "expo-linear-gradient";
import ModalContact from "../../components/addcontact/ModelContact";
import TextInputItem from "../../components/addcontact/TextInputItem";
import { KeyboardAwareScrollView } from "react-native-keyboard-aware-scroll-view";
import AuthContext from "../../store/AuthContext";
import { DuplicateInfoModel, DuplicateModel, FormInput } from "../../components/addcontact/ContextAddContact";
import LoadingDialog from "../../components/customDialog/dialog/loadingDialog/LoadingDialog";
import { useTranslation } from "react-i18next";


const AddContact = ({ contact, loading, navigation }) => {
  const { t, i18n } = useTranslation()
  const authCtx = useContext(AuthContext)
  const windowWidth = Dimensions.get("window").width;
  const windowHeight = Dimensions.get("window").height;
  const formRef = useRef();
  const ShimmerPlaceholder = createShimmerPlaceholder(LinearGradient);
  const [value, setValue] = useState({
    name: "",
    job_title: "",
    company: "",
    phone: "",
    email: "",
    fax: "",
    address: "",
    note: "",
    website: "",
    img_url: "",
  });
  useEffect(() => {
    if (contact && formRef.current) {
      formRef.current.setValues({
        name: contact.data.name,
        job_title: contact.data.job_title,
        company: contact.data.company,
        phone: contact.data.phone,
        email: contact.data.email,
        fax: contact.data.fax,
        address: contact.data.address,
        website: contact.data.website,
        img_url: contact.data.img_url,
      });
    }
  }, [contact]);

  const [duplicate, setDuplicate] = useState(false);
  const [contactId, setContactId] = useState();
  const [loadingDialog, setLoadingDialog] = useState(false);
  const [duplicateOther, setDuplicateOther] = useState(false);
  const [duplicateInfo, setDuplicateInfo] = useState({
    id_duplicate: "",
    owner: "",
  });

  const handelerModal = (item, name) => {
    if (formRef.current) {
      formRef.current.setValues({
        ...formRef.current.values,
        [name]: item,
      });
    }
  };

  const handleSubmit = (values) => {
    setLoadingDialog(true);
    FetchApi(ContactAPI.AddContact, Method.POST, ContentType.JSON, values, getMessage);
  };

  const getMessage = (data) => {
    setLoadingDialog(false);
    authCtx.checkToken()
    if(data){
      if (data.message === "D0001") {
        setDuplicate(true)
        setContactId(data.data.id)
      }
      if (data.message === "C0009") {
        navigation.dispatch(StackActions.popToTop());
        navigation.navigate('HomeSwap', { screen: 'ViewContact', params: { idContact: data.data.id, showFooter: true } })
      }
      if (data.message === "D0003") {
        setDuplicateOther(true)
        setDuplicateInfo({
          id_duplicate: data.data.id_duplicate,
          owner: data.data.user_name,
        })
      }
    }
  };

  const handleDuplicate = () => {
    navigation.dispatch(StackActions.popToTop());
    navigation.navigate("HomeSwap", {
      screen: "UpdateContact",
      params: { idContact: contactId },
    });
  }

  const handleDuplicateOther = () => {
    FetchApi(`${ContactAPI.RequestTransferContact}/${duplicateInfo.id}/${duplicateInfo.id_duplicate}`, Method.GET, ContentType.JSON, undefined, getMessageDuplaicate)
  }


  const handleOnCancel = () => {
    setDuplicateOther(false)
    navigation.dispatch(StackActions.popToTop());
    navigation.navigate("HomeSwap", {
      screen: "ViewContact",
      params: { idContact: duplicateInfo.id_duplicate, showFooter: true },
    });
  }

  return (
    <Provider style={styles.container}>
      <LoadingDialog visible={loadingDialog} />
      <ModalContact visible={duplicate} onPress={handleDuplicate} onPressVisable={() => setDuplicate(false)} context={DuplicateModel()} onCancel={() => setDuplicate(false)} />
      <ModalContact visible={duplicateOther} onPress={handleDuplicateOther} onPressVisable={() => setDuplicateOther(false)} context={DuplicateInfoModel(duplicateInfo.owner)} onCancel={handleOnCancel} />
      <View style={{ alignItems: "center" }}>
        <ShimmerPlaceholder visible={loading} width={windowWidth * 0.9} height={windowHeight * 0.3} shimmerStyle={{ borderRadius: 10, marginBottom: 10, }}>
          <View style={styles.imgContact}>
            {contact && formRef.current && formRef.current.values && <Image source={{ uri: formRef.current.values.img_url }} style={styles.image} />}
          </View>
        </ShimmerPlaceholder>
      </View>
      <Formik
        initialValues={value}
        onSubmit={handleSubmit}
        validationSchema={AddContactSchema}
        innerRef={formRef}
      >
        {({ handleChange, handleBlur, handleSubmit, values, errors, touched }) => {
          return (
            <View style={styles.formInput}>
              <KeyboardAwareScrollView contentContainerStyle={{ flexGrow: 1 }} >
                {FormInput().map((item, index) => {
                  return (
                    <View key={index} style={styles.formInput_component}>
                      <ShimmerPlaceholder
                        visible={loading}
                        style={{ width: "100%" }}
                        shimmerStyle={styles.shimmer_formInput}
                      >
                        {contact &&
                          <TextInputItem
                            item={item}
                            handleChange={handleChange}
                            handleBlur={handleBlur}
                            errors={errors}
                            touched={touched}
                            values={values}
                            loading={loading}
                            listItem={contact.data.Items.filter(word => word.length > 3)}
                            onPressRadio={handelerModal}
                          />}
                      </ShimmerPlaceholder>
                    </View>
                  );
                })}
                <View style={{ marginBottom: 15 }} />
              </KeyboardAwareScrollView>
              <View style={styles.footer}>
                <Button onPress={() => navigation.goBack()} style={styles.footer_button_label} color="#1890FF">{t("Screen_AddContact_FormInput_Button_Cancel")}</Button>
                <Button style={styles.footer_button_label} color="#1890FF" onPress={handleSubmit}>{t("Screen_AddContact_FormInput_Button_Add")}</Button>
              </View>
            </View>
          );
        }}
      </Formik>
    </Provider>
  );
};

export default AddContact;