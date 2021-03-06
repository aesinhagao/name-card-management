import { View, Text, Image, StyleSheet, TouchableOpacity } from "react-native";
import React from "react";
import { TextInput } from "react-native-paper";
const CustomInputs = ({
  value,
  setValue,
  secureTextEntry,
  label,
  icon,
  onpress,
}) => {
  return (
      <TextInput
          mode='outlined'
          label={label}
          value={value}
          onChangeText={setValue}
          secureTextEntry={secureTextEntry}
          style={styles.input}
          theme={{ roundness: 10, colors: { primary: '#1890FF', error: '#B22D1D' } }}
          right={<TextInput.Icon name={icon} onPress={onpress} />}
        />
  );
};

const styles = StyleSheet.create({
  input:{
    width: '85%',
    marginBottom: 5,
    backgroundColor: 'white'
  },
  icon: {},
});
export default CustomInputs;
