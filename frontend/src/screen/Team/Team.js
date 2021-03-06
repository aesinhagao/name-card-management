//import liraries
import React, { useEffect, useState, useCallback, useContext } from 'react';
import { View, SafeAreaView, ScrollView, Text, Alert } from 'react-native';
import styles from './styles';
import { Searchbar, FAB, Provider, Button } from 'react-native-paper';
import debounce from 'lodash.debounce';
import Tree from '../../components/team/Tree';
import { useIsFocused } from '@react-navigation/native';
import { FetchApi } from '../../service/api/FetchAPI';
import { ContentType, Method, TeamAPI } from '../../constants/ListAPI';
import AuthContext from '../../store/AuthContext';
import LoadingDialog from '../../components/customDialog/dialog/loadingDialog/LoadingDialog';

// create a component
const Team = ({ navigation }) => {
    const [text, setText] = useState("");
    const [team, setTeam] = useState([]);
    const [searchTeam, setSearchTeam] = useState([]);
    const [checked, setChecked] = useState(false);
    const [listExport, setListExport] = useState([]);
    const isFocused = useIsFocused()
    const authCtx = useContext(AuthContext);

    const [loading, setLoading] = useState(false);

    useEffect(() => {
        if (authCtx.role !== 1) {
            setLoading(true)
            FetchApi(TeamAPI.GetTeam, Method.GET, ContentType.JSON, undefined, getTeam)
        }
    }, [])

    useEffect(() => {
        FetchApi(TeamAPI.GetTeam, Method.GET, ContentType.JSON, undefined, getTeam)
    }, [isFocused]);

    const getTeam = (data) => {
        setTeam(data.data);
        setSearchTeam(data.data)
        setLoading(false)
    }

    const checklistExport = (item, check) => {
        if (check) {
            setListExport([...listExport, item]);
        } else {
            setListExport(listExport.filter(i => i !== item));
        }
    }

    const handleChecked = () => {
        setChecked(!checked)
    }
    const SearchApi = (value) => {
        FetchApi(`${TeamAPI.SearchMember}?value=${value}`, Method.GET, ContentType.JSON, undefined, getMember)
    }

    const getMember = (data) => {
        setSearchTeam(data.data)
    }

    const debounceSearch = useCallback(debounce((nextValue) => SearchApi(nextValue), 500), [])

    const handleSearch = (value) => {
        if (value === "") {
            setSearchTeam(team)
        }
        setText(value);
        debounceSearch(value);
    }

    const handleExport = () => {
        listExport.length === 0 && Alert.alert('Th??ng b??o', 'Vui l??ng ch???n nh??n vi??n', [{ text: 'OK' }])
        if (listExport.length > 0) {
            setLoading(true)
            FetchApi(TeamAPI.Export, Method.POST, ContentType.JSON, { array_id: listExport }, exportSuccess)
        } 
    }

    const exportSuccess = (data) => {
        console.log(data)
        setLoading(false)
        Alert.alert('Th??ng b??o', 'Xu???t file th??nh c??ng\nVui l??ng check email c???a b???n', [{ text: 'OK' }])
    }

    return (
        <Provider>
            <SafeAreaView style={styles.container}>
                <View style={styles.header}>
                    {authCtx.role !== 1 &&<View style={styles.sectionStyle}>
                        <Searchbar
                            placeholder="T??m ki???m nh??n vi??n"
                            theme={{
                                roundness: 10,
                                colors: { primary: '#1890FF' }
                            }}
                            value={text}
                            onChangeText={handleSearch}
                            clearIcon="close-circle"
                            editable={authCtx.role !== 1}
                        />
                    </View>}
                    {checked && <View style={styles.header_label}>
                        <Text style={styles.header_label_button}>???? ch???n ({listExport.length})</Text>
                        <Button uppercase={false} color="#1980FF" onPress={handleExport}>Export</Button>
                    </View>}
                </View>
                <View style={styles.body}>
                    {authCtx.role !== 1 && searchTeam && searchTeam.length === 0 && !loading &&
                        <View style={styles.container}>
                            <Text style={styles.label}>Kh??ng c?? th??nh vi??n</Text>
                        </View>
                    }
                    {authCtx.role === 1 &&
                        <View style={styles.container}>
                            <Text style={styles.label}>Kh??ng kh??? d???ng</Text>
                        </View>
                    }
                    {loading &&
                        <LoadingDialog onVisible={loading} />
                    }
                    {authCtx.role !== 1 &&
                        < ScrollView >
                            {searchTeam && searchTeam.map((item, index) => {
                                return (
                                    <Tree item={item} navigation={navigation} key={index} checked={checked} checklistExport={checklistExport} listExport={listExport} handleChecked={handleChecked} />
                                )
                            })}
                        </ScrollView>
                    }
                    {authCtx.role === 3 &&
                        <FAB icon="export" size={24} color="#FFF" style={styles.fab} onPress={() => setChecked(!checked)} />
                    }
                </View>
            </SafeAreaView >
        </Provider>
    );
};

export default Team;
