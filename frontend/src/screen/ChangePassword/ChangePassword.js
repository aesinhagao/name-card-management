import { View, StyleSheet, SafeAreaView } from "react-native";
import React, { useContext, useState } from "react";
import CustomInputs from "../../components/CustomInputs";
import CustomButtons from "../../components/CustomButtons";
import CustemHeaders from "../../components/CustomHeaders/CustemHeaders";
import Logo_ForgotPassword from "../../asset/image/forgotPassword.png";
import styles from "./styles";
import i18next from "../../language/i18n"; 
import AuthContext from "../../store/AuthContext";
import { useTranslation } from "react-i18next";
import { Searchbar, Appbar, Provider, Button } from "react-native-paper";
const ChangePassword = ({navigation}) => {
  const [user, setUser] = useState({
    email: "",
  });

  const onForgotPasswordPressed = () => {
    console.warn("onForgotPasswordPressed");
  };
  const { t, i18n } = useTranslation();
  const authCtx = useContext(AuthContext)
  i18n.changeLanguage(authCtx.locale)
  const onClearEmailPressed = () => {
    setUser({
      ...user,
      email: "",
    });
  };

  const handleChange = (name) => {
    return (text) => {
      setUser({
        ...user,
        [name]: text,
      });
    };
  };
  return (
    <SafeAreaView style={styles.container}>
      <Appbar.Header
          statusBarHeight={1}
          theme={{ colors: { primary: "transparent" } }}
        >
          <Appbar.BackAction onPress={() => navigation.goBack()} />
          <Appbar.Content title="Change Password" />
        </Appbar.Header> 
      <View style={styles.section}>    
     <CustomInputs
        value={user.email}
        setValue={handleChange("email")}
        icon={"close-circle-outline"}
        label={'Mật khẩu hiện tại'}
        onpress={onClearEmailPressed}
      />
        <CustomInputs
        value={user.email}
        setValue={handleChange("email")}
        icon={"close-circle-outline"}
        label={'Mật khẩu mới'}
        onpress={onClearEmailPressed}
      />
       <CustomInputs
        value={user.email}
        setValue={handleChange("email")}
        icon={"close-circle-outline"}
        label={'Nhập lại mật khẩu mới'}
        onpress={onClearEmailPressed}
      />
      <CustomButtons
        text={t("Screen_ForgotPassword_Button_ResetPassword")}
        onPress={onForgotPasswordPressed}
      />
   </View>
     
    </SafeAreaView>
  );
};

export default ChangePassword;
