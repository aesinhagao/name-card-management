import { View, Text, Image, StyleSheet, useWindowDimensions } from 'react-native'
import React, { useState, useContext } from 'react'
import AuthContext from '../../store/AuthContext';

import styles from './styles'

import CustomInputs from '../../components/CustomInputs'
import CustomButtons from '../../components/CustomButtons'
import CustemHeaders from '../../components/CustomHeaders/CustemHeaders'
const SignIn = ({ navigation }) => {
  const { username, setUsername } = useState('');
  const { password, setPassword } = useState('');

  const { height } = useWindowDimensions();

  const authCtx = useContext(AuthContext);

  const onForgotPasswordPressed = () => {
    navigation.navigate('ForgotPassword')
  }
  return (
      <View style={styles.root}>
        <CustemHeaders text_PRIMARY='Name Card Management' text_TERTIARY='Scan, Export & Manage' text_title='Đăng nhập' />
        <CustomInputs placeholder='Tên dăng nhập' value={username} setValue={setUsername} icon='cancel' />
        <CustomInputs placeholder='Mật khẩu' value={password} setValue={setPassword} secureTextEntry={true} icon='visibility-off' />
        <CustomButtons text='Quên mật khẩu' onPress={onForgotPasswordPressed} type="TERTIARY" />
        <CustomButtons text='Đăng nhập' onPress={authCtx.onLogin} />
      </View >


  )
}

export default SignIn