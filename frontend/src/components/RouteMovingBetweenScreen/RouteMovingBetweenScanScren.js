import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { ScanScreen } from '../../screen';
import SkeletonAddContact from '../Skeleton/SkeletonViewContact';
const ScanStack = createNativeStackNavigator()

const RouteMovingBetweenScanScreen = () => {
  return (
    <ScanStack.Navigator
      screenOptions={{
        headerShown: false,
      }}>
      <ScanStack.Screen name="Scan" component={ScanScreen} />
      <ScanStack.Screen name="AddContact" component={SkeletonAddContact} />
    </ScanStack.Navigator>
  )
}

export default RouteMovingBetweenScanScreen